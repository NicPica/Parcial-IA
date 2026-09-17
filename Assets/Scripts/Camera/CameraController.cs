using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    private enum Mode { FollowHunter, CycleAliveBoids }

    [Header("Referencias")]
    [SerializeField] private Transform hunter;
    private List<Boid> allBoids;

    [Header("Seguimiento")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -8f);
    [SerializeField] private float followSmoothness = 5f;
    [SerializeField] private float cycleInterval = 3f; // cada cuánto cambia de boid muerto

    private Mode currentMode = Mode.FollowHunter;
    private Transform currentCycleTarget;
    private float cycleTimer;
    private void Start()
    {
        allBoids = BoidSpawner.Instance != null ? BoidSpawner.Instance.GetAllBoids() : new List<Boid>();
    }

    public void SetFollowHunterMode()
    {
        currentMode = Mode.FollowHunter;
    }

    public void SetCycleAliveBoidsMode()
    {
        currentMode = Mode.CycleAliveBoids;
        cycleTimer = 0f;
        currentCycleTarget = null;
    }

    private void LateUpdate()
    {
        if (currentMode == Mode.FollowHunter)
        {
            FollowTarget(hunter);
        }
        else
        {
            TickCycleAliveBoids(); // o renombralo a TickCycleAliveBoids si cambiaste el nombre
        }
    }

    private void FollowTarget(Transform target)
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothness * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1f);
    }

    private void TickCycleAliveBoids()
    {
        // Si el objetivo actual ya no es válido (murió, se desactivó), buscar otro ya
        if (currentCycleTarget == null || !IsValidAliveBoidTarget(currentCycleTarget))
        {
            currentCycleTarget = FindNextAliveBoid();
            cycleTimer = 0f;
        }

        if (currentCycleTarget != null)
        {
            FollowTarget(currentCycleTarget);

            cycleTimer += Time.deltaTime;
            if (cycleTimer >= cycleInterval)
            {
                currentCycleTarget = FindNextAliveBoid(currentCycleTarget);
                cycleTimer = 0f;
            }
        }
    }

    private bool IsValidAliveBoidTarget(Transform target)
    {
        Boid boid = target.GetComponent<Boid>();
        return boid != null && !boid.IsDead && boid.gameObject.activeInHierarchy;
    }

    private Transform FindNextAliveBoid(Transform excluding = null)
    {
        List<Boid> aliveBoids = allBoids.FindAll(b => b != null && !b.IsDead && b.gameObject.activeInHierarchy);

        if (aliveBoids.Count == 0) return null;

        if (excluding != null && aliveBoids.Count > 1)
        {
            aliveBoids.RemoveAll(b => b.transform == excluding);
        }

        int randomIndex = Random.Range(0, aliveBoids.Count);
        return aliveBoids[randomIndex].transform;
    }
}