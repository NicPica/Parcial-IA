using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class HunterNPC : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Patrol")]
    [SerializeField] private WaypointPath path;
    [SerializeField] private bool returnToStartInsteadOfReverse = true;
    [SerializeField] private float waypointArriveThreshold = 0.5f;

    [Header("Detección")]
    [SerializeField] private float visionRadius = 8f;
    [SerializeField] private float visionScanInterval = 0.25f;
    [SerializeField] private LayerMask boidLayerMask;

    [Header("Combate")]
    [SerializeField] private float timeBetweenAttacks = 2f; // TBA
    [SerializeField] private float rangeAttackRadius = 5f;
    [SerializeField] private float meleeAttackRadius = 1.5f;

    [Header("Generación de POIs")]
    [SerializeField] private PointOfInterest poiPrefab;
    [SerializeField] private float poiSpawnInterval = 4f;
    [SerializeField] private int maxActivePOIs = 5;
    [SerializeField] private Vector3 poiSpawnAreaSize = new Vector3(20f, 0f, 20f);

    [Header("Referencias")]
    [SerializeField] private HunterVisuals visuals;

    private Rigidbody rb;
    private HunterStateMachine stateMachine;

    public int CurrentWaypointIndex { get; set; } = 0;
    public int PathDirection { get; set; } = 1; // 1 = adelante, -1 = atrás
    public float AttackTimer { get; set; } = 0f;
    public Transform CurrentTarget { get; set; }
    public Boid DeadBoidTarget { get; set; }
    public float PoiSpawnTimer { get; set; }

    public Rigidbody Rb => rb;
    public WaypointPath Path => path;
    public bool ReturnToStartInsteadOfReverse => returnToStartInsteadOfReverse;
    public float WaypointArriveThreshold => waypointArriveThreshold;
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    public float VisionRadius => visionRadius;
    public LayerMask BoidLayerMask => boidLayerMask;
    public float TimeBetweenAttacks => timeBetweenAttacks;
    public float RangeAttackRadius => rangeAttackRadius;
    public float MeleeAttackRadius => meleeAttackRadius;
    public PointOfInterest PoiPrefab => poiPrefab;
    public float PoiSpawnInterval => poiSpawnInterval;
    public int MaxActivePOIs => maxActivePOIs;
    public Vector3 PoiSpawnAreaSize => poiSpawnAreaSize;
    public HunterStateMachine StateMachine => stateMachine;
    public HunterVisuals Visuals => visuals;

    // Se refresca por intervalo: Update nunca consulta la física.
    public Transform CachedTarget { get; private set; }
    public Boid CachedDeadBoid { get; private set; }

    public string CurrentStateName => stateMachine != null ? stateMachine.CurrentID.ToString() : "None";

    private readonly Collider[] visionBuffer = new Collider[20];
    private WaitForSeconds visionWait;
    private int lastAliveCount = -1;
    private int lastDeadCount = -1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.freezeRotation = true;

        if (visuals == null) visuals = GetComponent<HunterVisuals>();

        stateMachine = new HunterStateMachine(this);
    }

    private void Start()
    {
        stateMachine.ChangeState(HunterStateID.Patrol);

        if (Bounds.Instance == null) Debug.LogWarning("[Hunter] No hay 'Bounds' en la escena; el NPC no quedará contenido dentro de los límites.");

        visionWait = new WaitForSeconds(visionScanInterval);
        StartCoroutine(VisionScanRoutine());
    }

    private void Update()
    {
        AttackTimer += Time.deltaTime;
        stateMachine.Update();

        ApplyBounds();
    }

    private IEnumerator VisionScanRoutine()
    {
        ScanVision();

        while (true)
        {
            yield return visionWait;
            ScanVision();
        }
    }

    private void ScanVision()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, visionRadius, visionBuffer, boidLayerMask);

        Transform closestAlive = null;
        float closestAliveDist = float.MaxValue;
        Boid nearestDead = null;
        float nearestDeadDist = float.MaxValue;
        int aliveCount = 0;
        int deadCount = 0;

        for (int i = 0; i < count; i++)
        {
            Boid boid = visionBuffer[i].GetComponent<Boid>();
            if (boid == null) continue;

            float dist = (visionBuffer[i].transform.position - transform.position).sqrMagnitude;

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
                    closestAlive = visionBuffer[i].transform;
                }
            }
        }

        CachedTarget = closestAlive;
        CachedDeadBoid = nearestDead;

        if (aliveCount != lastAliveCount || deadCount != lastDeadCount)
        {
            lastAliveCount = aliveCount;
            lastDeadCount = deadCount;
            Debug.Log($"[Hunter] escaneo por intervalo: {aliveCount} boid(s) vivo(s), {deadCount} eliminado(s) en visión");
        }
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

        Vector3 pos = transform.position + new Vector3(
            Random.Range(-poiSpawnAreaSize.x / 2f, poiSpawnAreaSize.x / 2f),
            0f,
            Random.Range(-poiSpawnAreaSize.z / 2f, poiSpawnAreaSize.z / 2f)
        );

        if (Bounds.Instance != null) pos = Bounds.Instance.CalculateBoundPosition(pos);

        Instantiate(poiPrefab, pos, Quaternion.identity);
    }

    private void ApplyBounds()
    {
        if (Bounds.Instance == null) return;

        Vector3 wrapped = Bounds.Instance.CalculateBoundPosition(rb.position);
        if (wrapped != rb.position) rb.position = wrapped;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, visionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, rangeAttackRadius);
    }
}
