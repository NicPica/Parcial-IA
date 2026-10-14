using UnityEngine;

public class PatrolState : HunterState
{
    public PatrolState(HunterNPC hunter) : base(hunter) { }

    public override void Enter()
    {
        hunter.PoiSpawnTimer = 0f;
        hunter.Visuals?.SetStateColor(HunterStateID.Patrol);
        Debug.Log($"[FSM] {hunter.name} -> Patrol");
    }

    public override void Update()
    {
        hunter.PoiSpawnTimer += Time.deltaTime;
        if (hunter.PoiSpawnTimer >= hunter.PoiSpawnInterval)
        {
            hunter.PoiSpawnTimer = 0f;
            hunter.TrySpawnPOI();
        }

        // Gather se evalúa antes que Attack: con un boid muerto en visión, recolectarlo manda.
        Boid deadBoid = hunter.CachedDeadBoid;
        if (deadBoid != null)
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

        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        if (hunter.Path == null || hunter.Path.Count == 0) return;

        Vector3 targetPos = hunter.Path.GetPosition(hunter.CurrentWaypointIndex);
        hunter.MoveTowards(targetPos);

        float dist = Vector3.Distance(hunter.transform.position, targetPos);
        if (dist <= hunter.WaypointArriveThreshold)
        {
            AdvanceWaypoint();
        }
    }

    private void AdvanceWaypoint()
    {
        int nextIndex = hunter.CurrentWaypointIndex + hunter.PathDirection;

        if (nextIndex >= hunter.Path.Count || nextIndex < 0)
        {
            if (hunter.ReturnToStartInsteadOfReverse)
            {
                hunter.CurrentWaypointIndex = 0;
            }
            else
            {
                hunter.PathDirection *= -1;
                hunter.CurrentWaypointIndex += hunter.PathDirection;
            }
        }
        else
        {
            hunter.CurrentWaypointIndex = nextIndex;
        }
    }

    public override void Exit() { }
}
