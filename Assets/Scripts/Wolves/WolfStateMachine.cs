public class WolfStateMachine : FiniteStateMachine<WolfStateID>
{
    public WolfStateMachine(Wolf wolf)
    {
        AddState(WolfStateID.Patrol, new WolfPatrolState(wolf));
        AddState(WolfStateID.Investigate, new WolfInvestigateState(wolf));
        AddState(WolfStateID.Chase, new WolfChaseState(wolf));
        AddState(WolfStateID.Search, new WolfSearchState(wolf));
    }
}
