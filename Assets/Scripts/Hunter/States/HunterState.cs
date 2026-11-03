public enum HunterStateID
{
    Search,
    Attack,
    Gather,
    Exit,
    Dead,
    Flee
}

public abstract class HunterState : State<HunterStateID>
{
    protected readonly HunterNPC hunter;

    protected HunterState(HunterNPC hunter)
    {
        this.hunter = hunter;
    }
}
