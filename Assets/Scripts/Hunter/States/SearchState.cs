using UnityEngine;

public class SearchState : HunterState
{
    public SearchState(HunterNPC hunter) : base(hunter) { }

    public override void Enter()
    {
        hunter.Visuals?.SetStateColor(HunterStateID.Search);
        if (hunter.LogStateChanges) Debug.Log($"[FSM] {hunter.name} -> Search");

        hunter.Follower.SetGoal(hunter.GetRandomCotoNode());
    }

    public override void Update()
    {
        // El temporizador no se reinicia al entrar: Attack y Gather lo interrumpen seguido.
        if (hunter.IsInCoto)
        {
            hunter.PoiSpawnTimer += Time.deltaTime;
            if (hunter.PoiSpawnTimer >= hunter.PoiSpawnInterval)
            {
                hunter.PoiSpawnTimer = 0f;
                hunter.TrySpawnPOI();
            }
        }

        // Gather se evalúa antes que Attack: con un boid muerto en visión, recolectarlo manda.
        Boid deadBoid = hunter.CachedDeadBoid;
        // El escaneo puede ser viejo: si ya lo recolectaron, entrar a Gather solo rebotaría.
        if (deadBoid != null && deadBoid.IsDead && deadBoid.gameObject.activeInHierarchy)
        {
            hunter.DeadBoidTarget = deadBoid;
            _fsm.ChangeState(HunterStateID.Gather);
            return;
        }

        if (hunter.AttackTimer >= hunter.TimeBetweenAttacks)
        {
            Transform target = hunter.CachedTarget;
            if (target != null)
            {
                hunter.CurrentTarget = target;
                _fsm.ChangeState(HunterStateID.Attack);
                return;
            }
        }

        if (hunter.Follower.TryGetNextPoint(out Vector3 point)) hunter.MoveTowards(point);
        else hunter.Follower.SetGoal(hunter.GetRandomCotoNode());
    }

    public override void Exit() { }
}
