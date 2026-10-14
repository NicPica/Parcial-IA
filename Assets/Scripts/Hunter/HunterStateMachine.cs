public class HunterStateMachine : FiniteStateMachine<HunterStateID>
{
    // Los estados se crean una sola vez y después las transiciones solo conmutan entre ellos.
    public HunterStateMachine(HunterNPC hunter)
    {
        AddState(HunterStateID.Patrol, new PatrolState(hunter));
        AddState(HunterStateID.Attack, new AttackState(hunter));
        AddState(HunterStateID.Gather, new GatherState(hunter));
    }
}
