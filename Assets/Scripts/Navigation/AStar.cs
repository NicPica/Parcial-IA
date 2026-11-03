using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    // Devuelve una lista vacía y costo infinito si no hay camino.
    public static List<GraphNode> FindPath(GraphNode start, GraphNode goal, out float cost, bool avoidSafeZones = false)
    {
        cost = float.PositiveInfinity;
        if (start == null || goal == null) return new();

        PriorityQueue<GraphNode> frontier = new();
        Dictionary<GraphNode, GraphNode> cameFrom = new() { { start, null } };
        Dictionary<GraphNode, float> costSoFar = new() { { start, 0f } };
        Vector3 goalPosition = goal.transform.position;

        frontier.Enqueue(start, 0f);
        while (frontier.Count > 0)
        {
            GraphNode current = frontier.Dequeue();
            if (current == goal)
            {
                cost = costSoFar[goal];
                return BuildPath(goal, cameFrom);
            }

            foreach (GraphNode neighbor in current.Neighbors)
            {
                if (avoidSafeZones && neighbor.IsSafeZone) continue;

                float newCost = costSoFar[current] + Vector3.Distance(current.transform.position, neighbor.transform.position);
                if (costSoFar.TryGetValue(neighbor, out float oldCost) && newCost >= oldCost) continue;

                costSoFar[neighbor] = newCost;
                cameFrom[neighbor] = current;
                frontier.Enqueue(neighbor, newCost + Vector3.Distance(neighbor.transform.position, goalPosition));
            }
        }

        return new();
    }

    // Compara por costo de camino, no en línea recta. Devuelve null si ninguno es alcanzable.
    public static GraphNode FindCheapestGoal(GraphNode start, IEnumerable<GraphNode> goals)
    {
        GraphNode cheapest = null;
        float cheapestCost = float.PositiveInfinity;

        foreach (GraphNode goal in goals)
        {
            FindPath(start, goal, out float cost);
            if (cost >= cheapestCost) continue;

            cheapest = goal;
            cheapestCost = cost;
        }

        return cheapest;
    }

    private static List<GraphNode> BuildPath(GraphNode goal, Dictionary<GraphNode, GraphNode> cameFrom)
    {
        List<GraphNode> path = new();
        for (GraphNode node = goal; node != null; node = cameFrom[node]) path.Add(node);
        path.Reverse();
        return path;
    }
}
