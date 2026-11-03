using UnityEngine;

// Va con A* a la Zona segura de menor costo y espera ahí; ver otro lobo no cambia la elección.
public class FleeState : HunterState
{
    private GraphNode safeZone;
    private float waitTimer;

    public FleeState(HunterNPC hunter) : base(hunter) { }

    public override void Enter()
    {
        waitTimer = 0f;
        hunter.CurrentTarget = null;
        hunter.DeadBoidTarget = null;
        hunter.Visuals?.SetStateColor(HunterStateID.Flee);
        GoToCheapestSafeZone();
        if (hunter.LogStateChanges) Debug.Log($"[FSM] {hunter.name} -> Flee, hacia {safeZone}");
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
            GoToCheapestSafeZone();
            return;
        }

        hunter.Stop();
        waitTimer += Time.deltaTime;
        if (waitTimer >= hunter.SafeZoneWaitTime)
        {
            _fsm.ChangeState(hunter.Gathers >= hunter.RequiredGathers ? HunterStateID.Exit : HunterStateID.Search);
        }
    }

    private void GoToCheapestSafeZone()
    {
        safeZone = hunter.GetCheapestSafeZone();
        hunter.ChosenSafeZone = safeZone;
        hunter.Follower.SetGoal(safeZone);
    }

    public override void Exit() => hunter.ChosenSafeZone = null;
}
