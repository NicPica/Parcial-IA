using UnityEngine;

public class ExitState : HunterState
{
    public ExitState(HunterNPC hunter) : base(hunter) { }

    public override void Enter()
    {
        hunter.Visuals?.SetStateColor(HunterStateID.Exit);
        if (hunter.LogStateChanges) Debug.Log($"[FSM] {hunter.name} -> Exit");

        hunter.Follower.SetGoal(hunter.ExitNode);
    }

    public override void Update()
    {
        if (hunter.Follower.TryGetNextPoint(out Vector3 point))
        {
            hunter.MoveTowards(point);
            return;
        }

        if (!hunter.Follower.HasArrived)
        {
            hunter.Follower.SetGoal(hunter.ExitNode);
            return;
        }

        hunter.Disappear();
        hunter.Respawn();
        _fsm.ChangeState(HunterStateID.Search);
    }

    public override void Exit() { }
}
