public class HunterStateMachine : FiniteStateMachine<HunterStateID>
{
    private readonly HunterNPC hunter;

    // Los estados se crean una sola vez y después las transiciones solo conmutan entre ellos.
    public HunterStateMachine(HunterNPC hunter)
    {
        this.hunter = hunter;

        AddState(HunterStateID.Search, new SearchState(hunter));
        AddState(HunterStateID.Attack, new AttackState(hunter));
        AddState(HunterStateID.Gather, new GatherState(hunter));
        AddState(HunterStateID.Exit, new ExitState(hunter));
        AddState(HunterStateID.Dead, new HunterDeadState(hunter));
        AddState(HunterStateID.Flee, new FleeState(hunter));
    }

    // Dead y Flee cortan a cualquier otro estado, sin que cada uno tenga que chequearlo.
    public override void Update()
    {
        if (hunter.IsCaught)
        {
            if (CurrentID != HunterStateID.Dead) ChangeState(HunterStateID.Dead);
        }
        else if (hunter.SeesWolf && !hunter.IsInSafeZone && CurrentID != HunterStateID.Flee && CurrentID != HunterStateID.Dead)
        {
            ChangeState(HunterStateID.Flee);
        }

        base.Update();
    }
}
