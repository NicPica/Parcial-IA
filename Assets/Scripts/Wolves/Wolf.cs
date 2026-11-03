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

    [Header("Debug")]
    [SerializeField] private bool logStateChanges;

    private Rigidbody rb;
    private WolfStateMachine stateMachine;

    public bool SeesHunter { get; private set; }
    public Vector3 LastKnownPosition { get; private set; }
    public bool HasAlert { get; set; }

    public PathFollower Follower { get; private set; }
    public WaypointPath Path => path;
    public bool ReturnToStartInsteadOfReverse => returnToStartInsteadOfReverse;
    public float WaypointArriveThreshold => waypointArriveThreshold;
    public float PatrolSpeed => patrolSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float SearchDuration => searchDuration;
    public WolfVisuals Visuals => visuals;
    public bool LogStateChanges => logStateChanges;
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

    private void OnEnable() => pack.Howled += OnHowl;

    private void OnDisable() => pack.Howled -= OnHowl;

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
        SeesHunter = sensor.Visible.Count > 0;
        if (SeesHunter) LastKnownPosition = sensor.Visible[0].transform.position;
    }

    // Solo guarda el dato: cada estado decide qué hacer con la alerta.
    private void OnHowl(Wolf source, Vector3 position)
    {
        if (source == this) return;

        LastKnownPosition = position;
        HasAlert = true;
    }

    public void Howl()
    {
        if (logStateChanges) Debug.Log($"[Wolf] {name} aúlla: Cazador en {LastKnownPosition}");
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
        if (other.TryGetComponent(out HunterNPC hunter) && sensor.InLineOfSight(other.transform.position)) hunter.Catch();
    }
}
