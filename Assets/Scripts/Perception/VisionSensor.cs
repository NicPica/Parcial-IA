using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Las comprobaciones de SensorAgent de clase, por intervalo: Update nunca consulta la física.
public class VisionSensor : MonoBehaviour
{
    [SerializeField] private float radius = 8f;
    [Tooltip("360 = sin ángulo de visión")]
    [SerializeField, Range(0f, 360f)] private float angle = 360f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float scanInterval = 0.25f;
    [SerializeField] private int maxResults = 20;

    private Collider[] buffer;
    private readonly List<Collider> visible = new();

    public float Radius => radius;
    public IReadOnlyList<Collider> Visible => visible;

    public event Action Scanned;

    private void Awake()
    {
        buffer = new Collider[maxResults];
    }

    private void OnEnable() => StartCoroutine(ScanRoutine());

    // enabled = false no frena las corrutinas: sin esto seguiría viendo mientras está apagado.
    private void OnDisable()
    {
        StopAllCoroutines();
        visible.Clear();
        Scanned?.Invoke();
    }

    public bool Sees(Transform target)
    {
        foreach (Collider seen in visible)
            if (seen != null && seen.transform == target) return true;

        return false;
    }

    private IEnumerator ScanRoutine()
    {
        WaitForSeconds wait = new(scanInterval);

        while (true)
        {
            yield return wait;
            Scan();
        }
    }

    private void Scan()
    {
        visible.Clear();
        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, buffer, targetMask);

        for (int i = 0; i < count; i++)
        {
            Vector3 position = buffer[i].transform.position;
            if (InViewAngle(position) && InLineOfSight(position)) visible.Add(buffer[i]);
        }

        Scanned?.Invoke();
    }

    private bool InViewAngle(Vector3 position)
    {
        return Vector3.Angle(transform.forward, position - transform.position) <= angle / 2f;
    }

    public bool InLineOfSight(Vector3 position)
    {
        return !Physics.Linecast(transform.position, position, obstacleMask, QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
