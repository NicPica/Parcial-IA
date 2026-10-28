using UnityEngine;

public class FeedState : BoidState
{
    private PointOfInterest feedingOn;

    public FeedState(Boid boid) : base(boid) { }

    public override void Enter()
    {
        feedingOn = null;
        boid.Visuals?.ShowFeeding();
        Debug.Log($"[Boid FSM] {boid.name} -> Feed");
    }

    public override void Update()
    {
        if (boid.HunterInSight)
        {
            _fsm.ChangeState(BoidStateID.Evade);
            return;
        }

        if (boid.TargetPOI == null)
        {
            _fsm.ChangeState(BoidStateID.Flocking);
            return;
        }

        boid.Move(boid.Arrive() + boid.Separation());
    }

    public void StartFeeding(PointOfInterest poi)
    {
        if (feedingOn == poi) return;

        feedingOn = poi;
        poi.BeginDamageOverTime(boid);
    }

    public override void Exit()
    {
        if (feedingOn != null) feedingOn.EndDamageOverTime(boid);
        feedingOn = null;
    }
}
