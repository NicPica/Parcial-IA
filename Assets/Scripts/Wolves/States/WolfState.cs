public enum WolfStateID
{
    Patrol,
    Investigate,
    Chase,
    Search
}

public abstract class WolfState : State<WolfStateID>
{
    protected readonly Wolf wolf;

    protected WolfState(Wolf wolf)
    {
        this.wolf = wolf;
    }

    protected void Announce(WolfStateID id)
    {
        wolf.Visuals.SetStateColor(id);
        if (wolf.LogStateChanges) UnityEngine.Debug.Log($"[Wolf] {wolf.name} -> {id}");
    }
}
