public class DeadState : BoidState
{
    public DeadState(Boid boid) : base(boid) { }

    public override void Enter()
    {
        boid.Stop();
        boid.SetKinematic(true);
        boid.Visuals?.ShowDead();
        boid.Visuals?.SetTrailEmitting(false);
    }

    // Sin transiciones propias: solo sale cuando el spawner llama a Boid.ResetState.
    public override void Update() { }

    public override void Exit()
    {
        boid.SetKinematic(false);
    }
}
