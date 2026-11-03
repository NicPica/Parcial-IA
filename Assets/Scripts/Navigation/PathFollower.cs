using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour
{
    [SerializeField]
    private NavGraph _graph;
    [SerializeField]
    private bool _avoidSafeZones;
    [SerializeField]
    private float _arriveDistance = 0.5f;
    [Tooltip("Si en este tiempo avanzó menos que Stuck Distance, recalcula el camino.")]
    [SerializeField]
    private float _stuckTime = 1f;
    [SerializeField]
    private float _stuckDistance = 0.2f;
    [SerializeField]
    private bool _drawPath = true;

    private List<GraphNode> _path = new();
    private GraphNode _goal;
    private int _index;
    private float _stuckTimer;
    private Vector3 _stuckCheckPosition;

    public NavGraph Graph => _graph;
    public bool HasArrived => _path.Count > 0 && _index >= _path.Count;

    // Corre A*: llamarlo solo al cambiar de objetivo, nunca por frame.
    public bool SetGoal(GraphNode goal)
    {
        Vector3 position = transform.position;

        _goal = goal;
        _index = 0;
        _stuckTimer = 0f;
        _stuckCheckPosition = position;
        _path = AStar.FindPath(_graph.GetClosestNode(position, _avoidSafeZones), goal, out _, _avoidSafeZones);

        // El nodo más cercano puede quedar atrás: si ya se ve el siguiente, no hay que volver.
        if (_path.Count > 1 && _graph.HasClearPath(position, _path[1].transform.position)) _index = 1;

        return _path.Count > 0;
    }

    // Devuelve false si no hay camino o si ya llegó.
    public bool TryGetNextPoint(out Vector3 point)
    {
        point = default;
        CheckStuck();

        while (_index < _path.Count)
        {
            point = _path[_index].transform.position;
            Vector3 offset = point - transform.position;
            offset.y = 0f;

            if (offset.magnitude > _arriveDistance) return true;
            _index++;
        }

        return false;
    }

    private void CheckStuck()
    {
        _stuckTimer += Time.deltaTime;
        if (_stuckTimer < _stuckTime) return;

        if (_index < _path.Count && Vector3.Distance(transform.position, _stuckCheckPosition) < _stuckDistance)
        {
            Debug.LogWarning($"[PathFollower] {name} se atascó yendo a {_goal.name}; recalculando.");
            SetGoal(_goal);
            return;
        }

        _stuckTimer = 0f;
        _stuckCheckPosition = transform.position;
    }

    private void OnDrawGizmos()
    {
        if (!_drawPath || _index >= _path.Count) return;

        Gizmos.color = Color.cyan;
        Vector3 from = transform.position;
        for (int i = _index; i < _path.Count; i++)
        {
            Gizmos.DrawLine(from, _path[i].transform.position);
            from = _path[i].transform.position;
        }
    }
}
