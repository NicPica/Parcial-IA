public enum HunterStateID
{
    Patrol,
    Attack,
    Gather
}

public abstract class HunterState : State<HunterStateID>
{
    protected readonly HunterNPC hunter;

    protected HunterState(HunterNPC hunter)
    {
        this.hunter = hunter;
    }
}
