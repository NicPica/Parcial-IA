using UnityEngine;

public class HunterDeadState : HunterState
{
    private float respawnTimer;

    public HunterDeadState(HunterNPC hunter) : base(hunter) { }

    public override void Enter()
    {
        respawnTimer = 0f;
        hunter.Disappear();
        if (hunter.LogStateChanges) Debug.Log($"[FSM] {hunter.name} -> Dead");
    }

    public override void Update()
    {
        respawnTimer += Time.deltaTime;
        if (respawnTimer < hunter.RespawnTime) return;

        hunter.Respawn();
        _fsm.ChangeState(HunterStateID.Search);
    }

    public override void Exit() { }
}
