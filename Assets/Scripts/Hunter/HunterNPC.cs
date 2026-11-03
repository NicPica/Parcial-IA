using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(PathFollower))]
public class HunterNPC : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Objetivo")]
    [SerializeField] private int requiredGathers = 3;
    [SerializeField] private float respawnTime = 5f;
    [SerializeField] private float safeZoneWaitTime = 3f;
    [SerializeField] private GraphNode startNode;
    [SerializeField] private GraphNode exitNode;

    [Header("Detección")]
    [SerializeField] private VisionSensor boidSensor;
    [SerializeField] private VisionSensor wolfSensor;

    [Header("Combate")]
    [SerializeField] private float timeBetweenAttacks = 2f; // TBA
    [SerializeField] private float rangeAttackRadius = 5f;
    [SerializeField] private float meleeAttackRadius = 1.5f;
    [SerializeField] private float attackWindUp = 0.3f; // delay para que el golpe se "sienta"

    [Header("Gather")]
    [SerializeField] private float gatherDuration = 1.5f;
    [SerializeField] private float gatherStopDistance = 1.2f;

    [Header("Generación de POIs")]
    [SerializeField] private PointOfInterest poiPrefab;
    [SerializeField] private float poiSpawnInterval = 4f;
    [SerializeField] private int maxActivePOIs = 5;

    [Header("Referencias")]
    [SerializeField] private HunterVisuals visuals;
    [SerializeField] private HunterStateIndicator indicator;

    [Header("Debug")]
    [SerializeField] private bool logStateChanges;
    [SerializeField] private bool drawGizmos = true;

    private Rigidbody rb;
    private HunterStateMachine stateMachine;
    private GraphNode[] cotoNodes;
    private GraphNode[] safeZoneNodes;
    private Collider hitbox;
    private Renderer[] renderers;
    private Canvas[] canvases;

    public int Gathers { get; set; }
    public bool IsCaught { get; private set; }
    public bool IsInSafeZone { get; set; }
    public float AttackTimer { get; set; } = 0f;
    public Transform CurrentTarget { get; set; }
    public Boid DeadBoidTarget { get; set; }
    public float PoiSpawnTimer { get; set; }

    public Rigidbody Rb => rb;
    public PathFollower Follower { get; private set; }
    public GraphNode ExitNode => exitNode;
    public int RequiredGathers => requiredGathers;
    public float RespawnTime => respawnTime;
    public float SafeZoneWaitTime => safeZoneWaitTime;
    public bool SeesWolf => wolfSensor.Visible.Count > 0;
    public bool IsInCoto => Bounds.Instance.Contains(transform.position);
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    public float VisionRadius => boidSensor.Radius;
    public float TimeBetweenAttacks => timeBetweenAttacks;
    public float RangeAttackRadius => rangeAttackRadius;
    public float MeleeAttackRadius => meleeAttackRadius;
    public float AttackWindUp => attackWindUp;
    public float GatherDuration => gatherDuration;
    public float GatherStopDistance => gatherStopDistance;
    public bool LogStateChanges => logStateChanges;
    public string SeenWolfName => SeesWolf ? wolfSensor.Visible[0].name : "ninguno";
    public GraphNode ChosenSafeZone { get; set; }
    public PointOfInterest PoiPrefab => poiPrefab;
    public float PoiSpawnInterval => poiSpawnInterval;
    public int MaxActivePOIs => maxActivePOIs;
    public HunterStateMachine StateMachine => stateMachine;
    public HunterVisuals Visuals => visuals;
    public HunterStateIndicator Indicator => indicator;

    // Los refresca el sensor en cada escaneo.
    public Transform CachedTarget { get; private set; }
    public Boid CachedDeadBoid { get; private set; }

    public string CurrentStateName => stateMachine != null ? stateMachine.CurrentID.ToString() : "None";

    private int lastAliveCount = -1;
    private int lastDeadCount = -1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Follower = GetComponent<PathFollower>();
        hitbox = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>();
        canvases = GetComponentsInChildren<Canvas>();
        boidSensor.Scanned += OnBoidScan;
        rb.useGravity = false;
        rb.freezeRotation = true;

        if (visuals == null) visuals = GetComponent<HunterVisuals>();

        stateMachine = new HunterStateMachine(this);
    }

    private void Start()
    {
        if (Bounds.Instance == null)
        {
            Debug.LogError("[Hunter] No hay 'Bounds' en la escena; sin ella no sabe dónde está el Coto.");
            enabled = false;
            return;
        }

        cotoNodes = System.Array.FindAll(Follower.Graph.Nodes, node => Bounds.Instance.Contains(node.transform.position));
        safeZoneNodes = System.Array.FindAll(Follower.Graph.Nodes, node => node.IsSafeZone);
        stateMachine.ChangeState(HunterStateID.Search);
    }

    private void Update()
    {
        AttackTimer += Time.deltaTime;
        stateMachine.Update();
    }

    private void OnBoidScan()
    {
        Transform closestAlive = null;
        float closestAliveDist = float.MaxValue;
        Boid nearestDead = null;
        float nearestDeadDist = float.MaxValue;
        int aliveCount = 0;
        int deadCount = 0;

        foreach (Collider seen in boidSensor.Visible)
        {
            Boid boid = seen.GetComponent<Boid>();
            if (boid == null) continue;

            float dist = (seen.transform.position - transform.position).sqrMagnitude;

            if (boid.IsDead)
            {
                deadCount++;
                if (dist < nearestDeadDist)
                {
                    nearestDeadDist = dist;
                    nearestDead = boid;
                }
            }
            else
            {
                aliveCount++;
                if (dist < closestAliveDist)
                {
                    closestAliveDist = dist;
                    closestAlive = seen.transform;
                }
            }
        }

        CachedTarget = closestAlive;
        CachedDeadBoid = nearestDead;

        if (logStateChanges && (aliveCount != lastAliveCount || deadCount != lastDeadCount))
        {
            lastAliveCount = aliveCount;
            lastDeadCount = deadCount;
            Debug.Log($"[Hunter] escaneo por intervalo: {aliveCount} boid(s) vivo(s), {deadCount} eliminado(s) en visión");
        }
    }

    public bool CanSee(Transform target) => boidSensor.Sees(target);

    public GraphNode GetRandomCotoNode() => cotoNodes[Random.Range(0, cotoNodes.Length)];

    public GraphNode GetCheapestSafeZone() => AStar.FindCheapestGoal(Follower.Graph.GetClosestNode(transform.position), safeZoneNodes);

    // La llama quien lo alcanza; la transición la decide la FSM.
    public void Catch() => IsCaught = true;

    public void Disappear()
    {
        Stop();
        Follower.ClearPath();
        SetPresent(false);
    }

    public void Respawn()
    {
        Vector3 position = startNode.transform.position;
        position.y = transform.position.y;
        transform.position = position;

        Gathers = 0;
        IsCaught = false;
        IsInSafeZone = false; // Apagar el collider no dispara OnTriggerExit.
        CurrentTarget = null;
        DeadBoidTarget = null;
        SetPresent(true);
    }

    // No usa SetActive(false): frenaría la FSM y el RespawnTime.
    private void SetPresent(bool present)
    {
        hitbox.enabled = present;
        boidSensor.enabled = present;
        wolfSensor.enabled = present;
        foreach (Renderer hunterRenderer in renderers) hunterRenderer.enabled = present;
        foreach (Canvas canvas in canvases) canvas.enabled = present;
    }

    public void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        rb.linearVelocity = direction.normalized * moveSpeed;

        Quaternion targetRot = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    public void Stop()
    {
        rb.linearVelocity = Vector3.zero;
    }

    public void TrySpawnPOI()
    {
        if (PointOfInterestManager.Instance == null || poiPrefab == null) return;
        if (PointOfInterestManager.Instance.ActiveCount >= maxActivePOIs) return;

        Vector3 pos = new Vector3(
            Random.Range(-Bounds.Instance.HalfWidth, Bounds.Instance.HalfWidth),
            transform.position.y,
            Random.Range(-Bounds.Instance.HalfHeight, Bounds.Instance.HalfHeight)
        );

        Instantiate(poiPrefab, pos, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || IsCaught) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, rangeAttackRadius);
    }
}
