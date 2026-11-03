using UnityEngine;

public class NavGraph : MonoBehaviour
{
    [SerializeField]
    private LayerMask _obstacleMask;
    [Tooltip("Radio del barrido; debe ser al menos el radio de los agentes para que no rocen las esquinas.")]
    [SerializeField]
    private float _connectionRadius = 0.75f;
    [SerializeField]
    private bool _drawGizmos = true;

    private GraphNode[] _nodes;

    public GraphNode[] Nodes => _nodes;

    private void Awake() => ConnectNodes();

    [ContextMenu("Conectar nodos")]
    public void ConnectNodes()
    {
        _nodes = GetComponentsInChildren<GraphNode>();
        foreach (GraphNode node in _nodes) node.ClearNeighbors();

        for (int i = 0; i < _nodes.Length; i++)
        {
            for (int j = i + 1; j < _nodes.Length; j++)
            {
                Vector3 a = _nodes[i].transform.position;
                Vector3 b = _nodes[j].transform.position;

                // Solo conexiones rectas, como en el mapa de la cátedra: sin diagonales.
                if (!Mathf.Approximately(a.x, b.x) && !Mathf.Approximately(a.z, b.z)) continue;
                if (!HasClearPath(a, b)) continue;
                if (HasNodeBetween(_nodes[i], _nodes[j])) continue;

                _nodes[i].AddNeighbor(_nodes[j]);
                _nodes[j].AddNeighbor(_nodes[i]);
            }
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
            foreach (GraphNode node in _nodes) UnityEditor.EditorUtility.SetDirty(node);
#endif
    }

    // Devuelve null si ningún nodo está a la vista.
    public GraphNode GetClosestNode(Vector3 position, bool avoidSafeZones = false)
    {
        GraphNode closest = null;
        float closestDistance = float.PositiveInfinity;

        foreach (GraphNode node in _nodes)
        {
            if (avoidSafeZones && node.IsSafeZone) continue;

            float distance = Vector3.Distance(position, node.transform.position);
            if (distance >= closestDistance || !HasClearPath(position, node.transform.position)) continue;

            closest = node;
            closestDistance = distance;
        }

        return closest;
    }

    public bool HasClearPath(Vector3 from, Vector3 to)
    {
        Vector3 offset = to - from;
        return !Physics.SphereCast(from, _connectionRadius, offset.normalized, out _, offset.magnitude, _obstacleMask, QueryTriggerInteraction.Ignore);
    }

    // Sin esto, los nodos de un mismo pasillo se conectarían todos entre sí.
    private bool HasNodeBetween(GraphNode a, GraphNode b)
    {
        Vector3 start = a.transform.position;
        Vector3 segment = b.transform.position - start;

        foreach (GraphNode node in _nodes)
        {
            if (node == a || node == b) continue;

            float t = Vector3.Dot(node.transform.position - start, segment) / segment.sqrMagnitude;
            if (t > 0f && t < 1f && Vector3.Distance(start + segment * t, node.transform.position) < _connectionRadius) return true;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        foreach (GraphNode node in GetComponentsInChildren<GraphNode>())
        {
            Gizmos.color = node.IsSafeZone ? Color.magenta : Color.white;
            Gizmos.DrawSphere(node.transform.position, 0.4f);
            foreach (GraphNode neighbor in node.Neighbors) Gizmos.DrawLine(node.transform.position, neighbor.transform.position);
        }
    }
}
