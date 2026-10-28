using UnityEngine;

public class EvadeState : BoidState
{
    private float timeInState;

    public EvadeState(Boid boid) : base(boid) { }

    public override void Enter()
    {
        timeInState = 0f;
        boid.Visuals?.ShowEvading();
        Debug.Log($"[Boid FSM] {boid.name} -> Evade");
    }

    public override void Update()
    {
        timeInState += Time.fixedDeltaTime;

        if (!boid.HunterInSight && timeInState >= boid.EvadeMinDuration)
        {
            _fsm.ChangeState(BoidStateID.Flocking);
            return;
        }

        // Sin Alignment ni Cohesion: huir manda, la Separation solo evita que se atraviesen.
        boid.Move(boid.Evade() + boid.Separation());
    }

    public override void Exit() { }
}
