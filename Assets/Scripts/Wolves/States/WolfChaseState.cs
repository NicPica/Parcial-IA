public class WolfChaseState : WolfState
{
    public WolfChaseState(Wolf wolf) : base(wolf) { }

    public override void Enter()
    {
        Announce(WolfStateID.Chase);
        wolf.Howl();
    }

    public override void Update()
    {
        if (!wolf.SeesHunter)
        {
            _fsm.ChangeState(WolfStateID.Investigate);
            return;
        }

        wolf.MoveTowards(wolf.LastKnownPosition, wolf.ChaseSpeed);
    }

    public override void Exit() { }
}
