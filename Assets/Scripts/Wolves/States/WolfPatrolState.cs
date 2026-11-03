using UnityEngine;

public class WolfPatrolState : WolfState
{
    private int waypointIndex;
    private int direction = 1;
    private bool usingPath;

    public WolfPatrolState(Wolf wolf) : base(wolf) { }

    public override void Enter()
    {
        Announce(WolfStateID.Patrol);
        StartLeg();
    }

    public override void Update()
    {
        if (wolf.SeesHunter)
        {
            _fsm.ChangeState(WolfStateID.Chase);
            return;
        }

        if (wolf.HasAlert)
        {
            _fsm.ChangeState(WolfStateID.Investigate);
            return;
        }

        if (usingPath)
        {
            if (wolf.Follower.TryGetNextPoint(out Vector3 point)) wolf.MoveTowards(point, wolf.PatrolSpeed);
            else StartLeg();
            return;
        }

        Vector3 waypoint = wolf.Path.GetPosition(waypointIndex);
        wolf.MoveTowards(waypoint, wolf.PatrolSpeed);

        Vector3 offset = waypoint - wolf.transform.position;
        offset.y = 0f;
        if (offset.magnitude <= wolf.WaypointArriveThreshold)
        {
            AdvanceWaypoint();
            StartLeg();
        }
    }

    // A* solo si el waypoint no está a la vista (1.8).
    private void StartLeg()
    {
        NavGraph graph = wolf.Follower.Graph;
        Vector3 waypoint = wolf.Path.GetPosition(waypointIndex);

        usingPath = !graph.HasClearPath(wolf.transform.position, waypoint);
        if (usingPath) wolf.Follower.SetGoal(graph.GetClosestNode(waypoint, true));
    }

    private void AdvanceWaypoint()
    {
        int next = waypointIndex + direction;

        if (next >= 0 && next < wolf.Path.Count)
        {
            waypointIndex = next;
        }
        else if (wolf.ReturnToStartInsteadOfReverse)
        {
            waypointIndex = 0;
        }
        else
        {
            direction *= -1;
            waypointIndex += direction;
        }
    }

    public override void Exit() { }
}
