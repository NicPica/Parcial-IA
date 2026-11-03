using System.Collections.Generic;
using UnityEngine;

public class GraphNode : MonoBehaviour
{
    [SerializeField]
    private bool _isSafeZone;
    [SerializeField]
    private List<GraphNode> _neighbors = new();

    public bool IsSafeZone => _isSafeZone;
    public IReadOnlyList<GraphNode> Neighbors => _neighbors;

    public void ClearNeighbors() => _neighbors.Clear();

    public void AddNeighbor(GraphNode neighbor) => _neighbors.Add(neighbor);
}
