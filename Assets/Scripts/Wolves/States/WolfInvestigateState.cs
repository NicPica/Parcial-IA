using UnityEngine;

// Va con A* a la última posición conocida, que queda fija hasta una nueva detección o alerta.
public class WolfInvestigateState : WolfState
{
    public WolfInvestigateState(Wolf wolf) : base(wolf) { }

    public override void Enter()
    {
        Announce(WolfStateID.Investigate);
        GoToLastKnownPosition();
    }

    public override void Update()
    {
        if (wolf.SeesHunter)
        {
            _fsm.ChangeState(WolfStateID.Chase);
            return;
        }

        if (wolf.HasAlert) GoToLastKnownPosition();

        if (wolf.Follower.TryGetNextPoint(out Vector3 point)) wolf.MoveTowards(point, wolf.ChaseSpeed);
        else _fsm.ChangeState(WolfStateID.Search);
    }

    private void GoToLastKnownPosition()
    {
        wolf.HasAlert = false;
        wolf.Follower.SetGoal(wolf.Follower.Graph.GetClosestNode(wolf.LastKnownPosition, true));
    }

    public override void Exit() { }
}
