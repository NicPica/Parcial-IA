using UnityEngine;

/// <summary>
/// Define un camino de patrulla como una lista ordenada de puntos en la escena.
/// </summary>
public class WaypointPath : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;

    public int Count => waypoints.Length;

    public Vector3 GetPosition(int index) => waypoints[index].position;

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.3f);

            int next = i + 1;
            if (next < waypoints.Length && waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
        }
    }
}