public class WolfStateMachine : FiniteStateMachine<WolfStateID>
{
    private readonly Wolf wolf;

    public WolfStateMachine(Wolf wolf)
    {
        this.wolf = wolf;

        AddState(WolfStateID.Patrol, new WolfPatrolState(wolf));
        AddState(WolfStateID.Investigate, new WolfInvestigateState(wolf));
        AddState(WolfStateID.Chase, new WolfChaseState(wolf));
        AddState(WolfStateID.Search, new WolfSearchState(wolf));
    }

    // Perder el objetivo corta a cualquier estado, sin que cada uno tenga que chequearlo.
    public override void Update()
    {
        if (wolf.HasLostTarget)
        {
            wolf.ForgetTarget();
            ChangeState(WolfStateID.Patrol);
        }

        base.Update();
    }
}
