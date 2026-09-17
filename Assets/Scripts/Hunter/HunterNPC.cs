using UnityEngine;
using System.Collections.Generic;

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

    private Rigidbody rb;
    private HunterStateMachine stateMachine;

    // Estado compartido entre los distintos IHunterState
    public int CurrentWaypointIndex { get; set; } = 0;
    public int PathDirection { get; set; } = 1; // 1 = adelante, -1 = atrás
    public float AttackTimer { get; set; } = 0f;
    public Transform CurrentTarget { get; set; }
    public Boid DeadBoidTarget { get; set; }
    public float PoiSpawnTimer { get; set; }

    // Referencias que necesitan los estados
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

    // Para feedback (lo usamos en el próximo paso de UI)
    public string CurrentStateName => stateMachine?.CurrentState?.StateName ?? "None";

    private readonly Collider[] visionBuffer = new Collider[20];

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.freezeRotation = true;

        stateMachine = new HunterStateMachine();
    }

    private void Start()
    {
        stateMachine.ChangeState(new PatrolState(this, stateMachine));
    }

    private void Update()
    {
        AttackTimer += Time.deltaTime;
        stateMachine.Tick();
    }

    /// <summary>
    /// Devuelve el primer boid vivo dentro del rango de percepción del Hunter, o null si no hay ninguno.
    /// </summary>
    public Transform GetClosestBoidInVision()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, visionRadius, visionBuffer, boidLayerMask);

        Transform closest = null;
        float closestDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Boid boid = visionBuffer[i].GetComponent<Boid>();
            if (boid == null || boid.IsDead) continue;

            float dist = (visionBuffer[i].transform.position - transform.position).sqrMagnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = visionBuffer[i].transform;
            }
        }

        return closest;
    }

    /// <summary>
    /// Busca un boid muerto (inactivo) dentro del rango de percepción.
    /// </summary>
    public Boid GetDeadBoidInVision()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, visionRadius, visionBuffer, boidLayerMask);

        for (int i = 0; i < count; i++)
        {
            Boid boid = visionBuffer[i].GetComponent<Boid>();
            if (boid != null && boid.IsDead)
                return boid;
        }

        return null;
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

        Instantiate(poiPrefab, pos, Quaternion.identity);
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