using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(PathFollower))]
public class Wolf : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.2f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Patrol")]
    [SerializeField] private WaypointPath path;
    [SerializeField] private bool returnToStartInsteadOfReverse;
    [SerializeField] private float waypointArriveThreshold = 0.5f;

    [Header("Search")]
    [SerializeField] private float searchDuration = 6f;

    [Header("Captura")]
    [Tooltip("Trigger que atrapa al Cazador: su radio es el radio de captura.")]
    [SerializeField] private SphereCollider captureTrigger;
    [SerializeField] private float captureRadius = 1.2f;

    [Header("Referencias")]
    [SerializeField] private VisionSensor sensor;
    [SerializeField] private WolfPack pack;
    [SerializeField] private WolfVisuals visuals;
    [SerializeField] private WolfStateIndicator indicator;

    [Header("Debug")]
    [SerializeField] private bool logStateChanges;

    private Rigidbody rb;
    private WolfStateMachine stateMachine;

    public bool SeesHunter { get; private set; }
    public Vector3 LastKnownPosition { get; private set; }
    public bool HasAlert { get; private set; }
    public bool HasLostTarget { get; private set; }

    public PathFollower Follower { get; private set; }
    public WaypointPath Path => path;
    public bool ReturnToStartInsteadOfReverse => returnToStartInsteadOfReverse;
    public float WaypointArriveThreshold => waypointArriveThreshold;
    public float PatrolSpeed => patrolSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float SearchDuration => searchDuration;
    public WolfVisuals Visuals => visuals;
    public bool LogStateChanges => logStateChanges;
    public WolfStateID CurrentState => stateMachine.CurrentID;
    public string CurrentStateName => stateMachine != null ? stateMachine.CurrentID.ToString() : "None";

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Follower = GetComponent<PathFollower>();
        rb.useGravity = false;
        rb.freezeRotation = true;
        captureTrigger.radius = captureRadius;
        sensor.Scanned += OnScan;
        stateMachine = new WolfStateMachine(this);
    }

    private void OnEnable()
    {
        pack.Howled += OnHowl;
        pack.TargetLost += OnTargetLost;
    }

    private void OnDisable()
    {
        pack.Howled -= OnHowl;
        pack.TargetLost -= OnTargetLost;
    }

    private void Start()
    {
        stateMachine.ChangeState(WolfStateID.Patrol);
    }

    private void Update()
    {
        stateMachine.Update();
    }

    // La posición se toma solo al escanear: el lobo sabe lo que percibió, nunca dónde está el Cazador ahora.
    private void OnScan()
    {
        // En una Zona segura no se lo detecta aunque se lo vea desde afuera.
        SeesHunter = sensor.Visible.Count > 0 && !sensor.Visible[0].GetComponent<HunterNPC>().IsInSafeZone;
        if (SeesHunter) LastKnownPosition = sensor.Visible[0].transform.position;
    }

    // Solo guarda el dato: cada estado decide qué hacer con la alerta.
    private void OnHowl(Wolf source, Vector3 position)
    {
        if (source == this) return;

        LastKnownPosition = position;
        HasAlert = true;
        indicator.ShowMessage("Alerta recibida");
    }

    private void OnTargetLost()
    {
        SeesHunter = false;
        HasLostTarget = true;
    }

    public void ClearAlert() => HasAlert = false;

    public void ForgetTarget()
    {
        LastKnownPosition = default;
        HasAlert = false;
        HasLostTarget = false;
    }

    public void Howl()
    {
        if (logStateChanges) Debug.Log($"[Wolf] {name} aúlla: Cazador en {LastKnownPosition}");
        indicator.ShowMessage("Detecta y aulla");
        pack.Howl(this, LastKnownPosition);
    }

    public void MoveTowards(Vector3 targetPosition, float speed)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        rb.linearVelocity = direction.normalized * speed;

        Quaternion targetRot = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Sin la línea de visión lo atraparía a través de las paredes finas de las Zonas seguras.
        if (!other.TryGetComponent(out HunterNPC hunter) || hunter.IsInSafeZone || !sensor.InLineOfSight(other.transform.position)) return;

        hunter.Catch();
        // Sin esto la jauría se queda donde lo atrapó y, de a poco, junto al spawn.
        pack.LoseTarget();
    }
}
