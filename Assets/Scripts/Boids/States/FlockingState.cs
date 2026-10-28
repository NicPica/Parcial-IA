using UnityEngine;

public class FlockingState : BoidState
{
    public FlockingState(Boid boid) : base(boid) { }

    public override void Enter()
    {
        boid.Visuals?.ShowFlocking();
        Debug.Log($"[Boid FSM] {boid.name} -> Flocking");
    }

    public override void Update()
    {
        if (boid.HunterInSight)
        {
            _fsm.ChangeState(BoidStateID.Evade);
            return;
        }

        if (boid.TargetPOI != null)
        {
            _fsm.ChangeState(BoidStateID.Feed);
            return;
        }

        // Sin vecinos las tres fuerzas de flocking dan cero, así que el Wander evita que el boid quede quieto.
        Vector3 steering = boid.Separation() + boid.Alignment() + boid.Cohesion();
        if (!boid.HasNeighbors) steering += boid.Wander();

        boid.Move(steering);
    }

    public override void Exit() { }
}
