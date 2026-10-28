public class BoidStateMachine : FiniteStateMachine<BoidStateID>
{
    private readonly FeedState feedState;

    // Igual que en el cazador, los estados se crean una sola vez y las transiciones solo conmutan entre ellos.
    public BoidStateMachine(Boid boid)
    {
        feedState = new FeedState(boid);

        AddState(BoidStateID.Flocking, new FlockingState(boid));
        AddState(BoidStateID.Evade, new EvadeState(boid));
        AddState(BoidStateID.Feed, feedState);
        AddState(BoidStateID.Dead, new DeadState(boid));
    }

    public void StartFeeding(PointOfInterest poi) => feedState.StartFeeding(poi);
}
