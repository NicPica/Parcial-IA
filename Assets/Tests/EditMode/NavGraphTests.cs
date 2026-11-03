using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

// Corre sobre el grafo real de la escena para verificar los nodos puestos a mano.
public class NavGraphTests
{
    private const string ScenePath = "Assets/Scenes/EscenaPrincipal.unity";

    private NavGraph _graph;
    private LayerMask _obstacleMask;

    [SetUp]
    public void SetUp()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Physics.SyncTransforms();
        _graph = Object.FindAnyObjectByType<NavGraph>();
        _obstacleMask = LayerMask.GetMask("Obstacle");
        _graph.ConnectNodes();
    }

    [Test]
    public void AStarCostMatchesDijkstraForEveryPair()
    {
        foreach (GraphNode start in _graph.Nodes)
        {
            Dictionary<GraphNode, float> expected = Dijkstra(start);
            foreach (GraphNode goal in _graph.Nodes)
            {
                List<GraphNode> path = AStar.FindPath(start, goal, out float cost);
                Assert.AreEqual(expected[goal], cost, 0.001f, $"{start.name} -> {goal.name}");
                Assert.AreEqual(start, path[0]);
                Assert.AreEqual(goal, path[^1]);
            }
        }
    }

    [Test]
    public void AvoidingSafeZonesNeverUsesSafeNodesAndReachesEveryOtherNode()
    {
        GraphNode[] open = _graph.Nodes.Where(n => !n.IsSafeZone).ToArray();
        Assert.AreEqual(2, _graph.Nodes.Length - open.Length);

        foreach (GraphNode start in open)
        {
            foreach (GraphNode goal in open)
            {
                List<GraphNode> path = AStar.FindPath(start, goal, out _, true);
                Assert.IsNotEmpty(path, $"{start.name} -> {goal.name}");
                Assert.IsFalse(path.Any(n => n.IsSafeZone), $"{start.name} -> {goal.name}");
            }
        }

        GraphNode safe = _graph.Nodes.First(n => n.IsSafeZone);
        Assert.IsEmpty(AStar.FindPath(open[0], safe, out float cost, true));
        Assert.IsTrue(float.IsPositiveInfinity(cost));
    }

    [Test]
    public void ConnectionsAreSymmetricAndNeverCrossWalls()
    {
        foreach (GraphNode node in _graph.Nodes)
        {
            Vector3 from = node.transform.position;
            Assert.IsFalse(Physics.CheckSphere(from, 0.6f, _obstacleMask), $"{node.name} está dentro de una pared");

            foreach (GraphNode neighbor in node.Neighbors)
            {
                Vector3 to = neighbor.transform.position;
                Assert.IsTrue(neighbor.Neighbors.Contains(node), $"{node.name} <-> {neighbor.name}");
                Assert.IsFalse(Physics.Linecast(from, to, _obstacleMask), $"{node.name} -> {neighbor.name}");
            }
        }
    }

    [Test]
    public void EveryFreePointSeesANode()
    {
        // Barre el piso caminable: un agente en cualquier lado debe poder entrar al grafo.
        const float agentRadius = 0.5f;
        float y = _graph.Nodes[0].transform.position.y;

        for (float x = -40f; x <= 40f; x += 1f)
        {
            for (float z = -26f; z <= 26f; z += 1f)
            {
                Vector3 point = new(x, y, z);
                if (Physics.CheckSphere(point, agentRadius, _obstacleMask)) continue;

                Assert.IsNotNull(_graph.GetClosestNode(point), $"{point} no ve ningún nodo");
            }
        }
    }

    [Test]
    public void PathAcrossACornerTheAgentIsTouchingIsNotClear()
    {
        // Un lobo quedó trabado acá: la esquina de la pared estaba dentro del radio del barrido.
        Vector3 touchingCorner = new(25.5f, 1.85f, 2.14f);
        GraphNode beyondCorner = _graph.Nodes.First(n => n.name == "Node_PasilloEste");

        Assert.IsFalse(_graph.HasClearPath(touchingCorner, beyondCorner.transform.position));
    }

    [Test]
    public void FollowerWalksTheWholePathToTheGoal()
    {
        PathFollower follower = Object.FindAnyObjectByType<PathFollower>();
        GraphNode start = _graph.Nodes.First(n => n.name == "Node_Inicio");
        GraphNode goal = _graph.Nodes.First(n => n.name == "Node_Salida");

        follower.transform.position = start.transform.position;
        Assert.IsTrue(follower.SetGoal(goal));
        Assert.IsFalse(follower.HasArrived);

        Vector3 last = follower.transform.position;
        for (int i = 0; i < _graph.Nodes.Length && follower.TryGetNextPoint(out Vector3 point); i++)
        {
            Assert.IsFalse(Physics.Linecast(last, point, _obstacleMask), $"{last} -> {point}");
            follower.transform.position = last = point;
        }

        Assert.IsTrue(follower.HasArrived);
        Assert.AreEqual(goal.transform.position, follower.transform.position);

        // Sin objetivo no hay camino, y eso no cuenta como haber llegado.
        Assert.IsFalse(follower.SetGoal(null));
        Assert.IsFalse(follower.HasArrived);
        Assert.IsFalse(follower.TryGetNextPoint(out _));
    }

    [Test]
    public void CheapestSafeZoneIsChosenByPathCostNotStraightLine()
    {
        GraphNode[] safe = _graph.Nodes.Where(n => n.IsSafeZone).ToArray();
        int straightLineWouldFail = 0;

        foreach (GraphNode start in _graph.Nodes)
        {
            Dictionary<GraphNode, float> costs = Dijkstra(start);
            GraphNode cheapest = AStar.FindCheapestGoal(start, safe);
            Assert.AreEqual(safe.Min(n => costs[n]), costs[cheapest], 0.001f, start.name);

            GraphNode nearest = safe.OrderBy(n => Vector3.Distance(start.transform.position, n.transform.position)).First();
            if (costs[nearest] > costs[cheapest] + 0.001f) straightLineWouldFail++;
        }

        Assert.Greater(straightLineWouldFail, 0, "El grafo no tiene ningún caso que distinga camino de línea recta");
    }

    private Dictionary<GraphNode, float> Dijkstra(GraphNode start)
    {
        Dictionary<GraphNode, float> costs = _graph.Nodes.ToDictionary(n => n, _ => float.PositiveInfinity);
        costs[start] = 0f;
        HashSet<GraphNode> pending = new(_graph.Nodes);

        while (pending.Count > 0)
        {
            GraphNode current = pending.OrderBy(n => costs[n]).First();
            pending.Remove(current);

            foreach (GraphNode neighbor in current.Neighbors)
            {
                float cost = costs[current] + Vector3.Distance(current.transform.position, neighbor.transform.position);
                if (cost < costs[neighbor]) costs[neighbor] = cost;
            }
        }

        return costs;
    }
}
