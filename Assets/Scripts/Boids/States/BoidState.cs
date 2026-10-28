public enum BoidStateID
{
    Flocking,
    Evade,
    Feed,
    Dead
}

public abstract class BoidState : State<BoidStateID>
{
    protected readonly Boid boid;

    protected BoidState(Boid boid)
    {
        this.boid = boid;
    }
}
