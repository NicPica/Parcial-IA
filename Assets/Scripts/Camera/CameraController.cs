using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    private enum Mode { Overview, FollowHunter, CycleAliveBoids }

    [Header("Referencias")]
    [SerializeField] private Transform hunter;
    private List<Boid> allBoids;

    [Header("Vista general")]
    [SerializeField] private Vector3 overviewPosition = new Vector3(0f, 48f, 0f);
    [SerializeField] private Vector3 overviewRotation = new Vector3(90f, 0f, 0f);

    [Header("Seguimiento")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -8f);
    [SerializeField] private float followSmoothness = 5f;
    [SerializeField] private float cycleInterval = 3f; // cada cuánto cambia de boid muerto

    private Mode currentMode = Mode.Overview;
    private Transform currentCycleTarget;
    private float cycleTimer;
    private void Start()
    {
        allBoids = BoidSpawner.Instance != null ? BoidSpawner.Instance.GetAllBoids() : new List<Boid>();
    }

    public void SetOverviewMode()
    {
        currentMode = Mode.Overview;
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
        if (currentMode == Mode.Overview)
        {
            float step = followSmoothness * Time.deltaTime;
            transform.SetPositionAndRotation(
                Vector3.Lerp(transform.position, overviewPosition, step),
                Quaternion.Slerp(transform.rotation, Quaternion.Euler(overviewRotation), step));
        }
        else if (currentMode == Mode.FollowHunter)
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