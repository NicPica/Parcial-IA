using UnityEngine;
using System.Collections;

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
    [SerializeField] private float poiDetectionRadius = 12f;
    [SerializeField] private float perceptionScanInterval = 0.25f;

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

    [Header("Referencias")]
    [SerializeField] private BoidVisuals visuals;

    private Rigidbody rb;
    private Vector3 currentVelocity;
    private Transform hunterInSight;
    private PointOfInterest targetPOI;

    private bool isDead = false;

    private int validNeighborCount = 0;
    private readonly Collider[] neighborBuffer = new Collider[20];
    private readonly Collider[] hunterBuffer = new Collider[20];
    private readonly Transform[] neighborTransforms = new Transform[20];
    private readonly Vector3[] neighborPositions = new Vector3[20];
    private readonly Vector3[] neighborVelocities = new Vector3[20];

    private WaitForSeconds perceptionWait;
    private bool wasEvading = false;

    public bool IsDead => isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.freezeRotation = true;
        currentHealth = maxHealth;

        if (visuals == null) visuals = GetComponent<BoidVisuals>();

        if (Bounds.Instance == null) Debug.LogWarning("[Boid] No hay 'Bounds' en la escena; el agente no quedará contenido dentro de los límites.");

        perceptionWait = new WaitForSeconds(perceptionScanInterval);
    }

    private void OnEnable()
    {
        StartCoroutine(PerceptionScanRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        hunterInSight = null;
        wasEvading = false;
    }

    private IEnumerator PerceptionScanRoutine()
    {
        ScanPerception();

        while (true)
        {
            yield return perceptionWait;
            ScanPerception();
        }
    }

    private void ScanPerception()
    {
        int neighborCount = Physics.OverlapSphereNonAlloc(transform.position, flockRadius, neighborBuffer, boidLayer);

        validNeighborCount = 0;
        for (int i = 0; i < neighborCount; i++)
        {
            if (neighborBuffer[i] == null) continue;
            if (neighborBuffer[i].gameObject == gameObject) continue;
            if (validNeighborCount >= neighborTransforms.Length) break;

            Transform other = neighborBuffer[i].transform;
            if (other == null) continue;

            neighborTransforms[validNeighborCount] = other;
            neighborPositions[validNeighborCount] = other.position;

            Rigidbody otherRb = neighborBuffer[i].attachedRigidbody;
            neighborVelocities[validNeighborCount] = otherRb != null ? otherRb.linearVelocity : Vector3.zero;

            validNeighborCount++;
        }

        int hunterCount = Physics.OverlapSphereNonAlloc(transform.position, hunterVisionRadius, hunterBuffer, hunterLayer);
        hunterInSight = hunterCount > 0 && hunterBuffer[0] != null ? hunterBuffer[0].transform : null;

        bool evading = hunterInSight != null;
        if (evading != wasEvading)
        {
            wasEvading = evading;
            Debug.Log($"[Boid] {name} {(evading ? "detectó al Hunter -> Evade" : "perdió al Hunter -> Flocking")}");
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        Vector3 steering = ComputeFlockingAndArrive();

        if (hunterInSight != null)
        {
            // El Evade se suma al flocking en vez de reemplazarlo: mantiene la prioridad por
            // su peso y la Separation sigue activa, así no se atraviesan al huir.
            steering += SteeringBehaviors.Evade(transform.position, hunterInSight.position) * evadeWeight;
            if (visuals != null) visuals.ShowEvading();
        }
        else if (visuals != null)
        {
            visuals.ShowFlocking();
        }

        steering.y = 0f;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        currentVelocity = Vector3.ClampMagnitude(currentVelocity + steering * Time.fixedDeltaTime, maxSpeed);
        currentVelocity.y = 0f;

        rb.linearVelocity = currentVelocity;

        ApplyBounds();

        if (currentVelocity.sqrMagnitude > 0.01f)
            transform.forward = currentVelocity.normalized;
    }

    private Vector3 ComputeFlockingAndArrive()
    {
        Vector3 separation = SteeringBehaviors.Separation(transform.position, neighborTransforms, validNeighborCount, separationRadius) * separationWeight;
        Vector3 alignment = SteeringBehaviors.Alignment(neighborVelocities, validNeighborCount) * alignmentWeight;
        Vector3 cohesion = SteeringBehaviors.Cohesion(transform.position, neighborPositions, validNeighborCount) * cohesionWeight;

        if (!HasTargetWithinDetectionRadius())
        {
            targetPOI = PointOfInterestManager.Instance != null
                ? PointOfInterestManager.Instance.GetClosestActivePOI(transform.position, poiDetectionRadius)
                : null;
        }

        bool hasTarget = targetPOI != null;

        Vector3 arrive = hasTarget
            ? SteeringBehaviors.Arrive(transform.position, targetPOI.transform.position, currentVelocity, maxSpeed, arriveSlowRadius, arriveStopRadius) * arriveWeight
            : Vector3.zero;

        // Con un objetivo dentro del radio se baja la influencia de la bandada para que
        // lleguen al POI en vez de orbitar entre ellos.
        float flockingScale = hasTarget ? 0.4f : 1f;

        return separation + (alignment + cohesion) * flockingScale + arrive;
    }

    private bool HasTargetWithinDetectionRadius()
    {
        if (targetPOI == null || !targetPOI.IsAlive) return false;

        return (targetPOI.transform.position - transform.position).sqrMagnitude <= poiDetectionRadius * poiDetectionRadius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (targetPOI != null && other.gameObject == targetPOI.gameObject)
        {
            targetPOI.BeginDamageOverTime(this);
        }
    }

    public void Die()
    {
        isDead = true;
        currentVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (visuals != null)
        {
            visuals.ShowDead();
            visuals.SetTrailEmitting(false);
        }
    }

    public void Collect()
    {
        gameObject.SetActive(false);
        BoidSpawner.Instance?.ScheduleRespawn(this);
    }

    private void ApplyBounds()
    {
        if (Bounds.Instance == null) return;

        Vector3 wrapped = Bounds.Instance.CalculateBoundPosition(rb.position);
        if (wrapped != rb.position) rb.position = wrapped;
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

        if (visuals != null) visuals.ResetToFlocking();
    }
}
