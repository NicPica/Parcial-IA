using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Boid : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float maxForce = 10f;

    [Header("Radios de percepción")]
    [SerializeField] private float separationRadius = 1.5f;   // menor a propósito
    [SerializeField] private float flockRadius = 4f;          // alignment + cohesion
    [SerializeField] private float hunterVisionRadius = 6f;

    [Header("Pesos de comportamiento")]
    [SerializeField] private float separationWeight = 1.5f;
    [SerializeField] private float alignmentWeight = 1f;
    [SerializeField] private float cohesionWeight = 1f;
    [SerializeField] private float evadeWeight = 3f;
    [SerializeField] private float arriveWeight = 1.2f;
    [SerializeField] private float arriveSlowRadius = 3f;
    [SerializeField] private float arriveStopRadius = 1.5f;

    [Header("Vida")]
    [SerializeField] private float maxHealth = 3f;
    private float currentHealth;

    [Header("Capas")]
    [SerializeField] private LayerMask boidLayer;
    [SerializeField] private LayerMask hunterLayer;

    private Rigidbody rb;
    private Vector3 currentVelocity;
    private Transform hunterInSight;
    private PointOfInterest targetPOI;

    private bool isDead = false;

    // Buffers reusables para no generar garbage cada frame
    private readonly Collider[] neighborBuffer = new Collider[20];
    private readonly Transform[] neighborTransforms = new Transform[20];
    private readonly Vector3[] neighborPositions = new Vector3[20];
    private readonly Vector3[] neighborVelocities = new Vector3[20];

    public bool IsDead => isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.freezeRotation = true;
        currentHealth = maxHealth;
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        DetectHunter();

        Vector3 steering;

        if (hunterInSight != null)
        {
            // Evade tiene prioridad absoluta mientras haya amenaza
            steering = SteeringBehaviors.Evade(transform.position, hunterInSight.position) * evadeWeight;
        }
        else
        {
            steering = ComputeFlockingAndArrive();
        }

        steering = Vector3.ClampMagnitude(steering, maxForce);
        currentVelocity = Vector3.ClampMagnitude(currentVelocity + steering * Time.fixedDeltaTime, maxSpeed);

        rb.linearVelocity = currentVelocity;

        if (currentVelocity.sqrMagnitude > 0.01f)
            transform.forward = currentVelocity.normalized;
    }

    private Vector3 ComputeFlockingAndArrive()
    {
        int neighborCount = Physics.OverlapSphereNonAlloc(transform.position, flockRadius, neighborBuffer, boidLayer);

        int validCount = 0;
        for (int i = 0; i < neighborCount; i++)
        {
            if (neighborBuffer[i].gameObject == gameObject) continue;
            neighborTransforms[validCount] = neighborBuffer[i].transform;
            neighborPositions[validCount] = neighborBuffer[i].transform.position;

            Rigidbody otherRb = neighborBuffer[i].attachedRigidbody;
            neighborVelocities[validCount] = otherRb != null ? otherRb.linearVelocity : Vector3.zero;
            validCount++;
        }

        Vector3 separation = SteeringBehaviors.Separation(transform.position, neighborTransforms, validCount, separationRadius) * separationWeight;
        Vector3 alignment = SteeringBehaviors.Alignment(neighborVelocities, validCount) * alignmentWeight;
        Vector3 cohesion = SteeringBehaviors.Cohesion(transform.position, neighborPositions, validCount) * cohesionWeight;

        Vector3 arrive = Vector3.zero;
        bool hasTarget = targetPOI != null && targetPOI.IsAlive;

        if (hasTarget)
        {
            arrive = SteeringBehaviors.Arrive(transform.position, targetPOI.transform.position, currentVelocity, maxSpeed, arriveSlowRadius, arriveStopRadius) * arriveWeight;
        }
        else
        {
            targetPOI = PointOfInterestManager.Instance?.GetClosestActivePOI(transform.position);
        }

        // Cuando hay un objetivo activo, reducimos la influencia de la bandada
        // para que prioricen llegar en vez de orbitar entre ellos.
        float flockingScale = hasTarget ? 0.4f : 1f;

        return (separation) + (alignment + cohesion) * flockingScale + arrive;
    }

    private void DetectHunter()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, hunterVisionRadius, hunterLayer);
        hunterInSight = hits.Length > 0 ? hits[0].transform : null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (targetPOI != null && other.gameObject == targetPOI.gameObject)
        {
            targetPOI.BeginDamageOverTime(this);
        }
    }

    public void TakeDamageFromHunterInteraction()
    {
        // reservado si en el futuro el hunter daña directamente al boid
    }

    public void Die()
    {
        isDead = true;
        currentVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true; // queda inmóvil en el lugar
    }

    public void Collect()
    {
        gameObject.SetActive(false);
        BoidSpawner.Instance?.ScheduleRespawn(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, flockRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hunterVisionRadius);
    }
    public void ResetState()
    {
        isDead = false;
        currentHealth = maxHealth;
        currentVelocity = Vector3.zero;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        targetPOI = null;
    }
}