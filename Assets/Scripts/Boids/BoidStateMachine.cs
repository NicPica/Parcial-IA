public class BoidStateMachine : FiniteStateMachine<BoidStateID>
{
    private readonly Boid boid;
    private readonly FeedState feedState;

    // Igual que en el cazador, los estados se crean una sola vez y las transiciones solo conmutan entre ellos.
    public BoidStateMachine(Boid boid)
    {
        this.boid = boid;
        feedState = new FeedState(boid);

        AddState(BoidStateID.Flocking, new FlockingState(boid));
        AddState(BoidStateID.Evade, new EvadeState(boid));
        AddState(BoidStateID.Feed, feedState);
        AddState(BoidStateID.Dead, new DeadState(boid));
    }

    // Morir y reaparecer las decide la propia FSM: de afuera solo cambia IsDead.
    public override void Update()
    {
        if (boid.IsDead != (CurrentID == BoidStateID.Dead))
            ChangeState(boid.IsDead ? BoidStateID.Dead : BoidStateID.Flocking);

        base.Update();
    }

    public void StartFeeding(PointOfInterest poi) => feedState.StartFeeding(poi);
}
