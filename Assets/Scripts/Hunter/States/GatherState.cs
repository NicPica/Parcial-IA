using UnityEngine;

/// <summary>
/// El Hunter se dirige hacia un boid eliminado, lo recolecta durante un tiempo
/// determinado, y lo hace desaparecer (lo que dispara su respawn en el BoidSpawner).
/// </summary>
public class GatherState : HunterState
{
    private const float GatherDuration = 1.5f;
    private const float GatherStopDistance = 1.2f;

    private float gatherTimer;

    public GatherState(HunterNPC hunter) : base(hunter) { }

    public override void Enter()
    {
        gatherTimer = 0f;
        hunter.Visuals?.SetStateColor(HunterStateID.Gather);
        Debug.Log($"[FSM] {hunter.name} -> Gather");
    }

    public override void Update()
    {
        Boid target = hunter.DeadBoidTarget;

        // El objetivo dejó de estar disponible (fue recolectado por otro medio,
        // reapareció, o el objeto fue destruido) -> abandonar y volver a Patrol
        if (target == null || !target.gameObject.activeInHierarchy || !target.IsDead)
        {
            AbandonGather();
            return;
        }

        float dist = Vector3.Distance(hunter.transform.position, target.transform.position);

        if (dist > GatherStopDistance)
        {
            // Todavía no llegó: se dirige hacia el boid eliminado
            gatherTimer = 0f;
            hunter.MoveTowards(target.transform.position);
            return;
        }

        // Llegó: ejecuta la acción de recolección durante GatherDuration segundos
        hunter.Stop();
        gatherTimer += Time.deltaTime;

        if (gatherTimer >= GatherDuration)
        {
            CompleteGather(target);
        }
    }

    private void CompleteGather(Boid target)
    {
        target.Collect();
        GameStatsManager.Instance?.RegisterBoidCaught();
        hunter.DeadBoidTarget = null;
        _fsm.ChangeState(HunterStateID.Patrol);
    }

    private void AbandonGather()
    {
        hunter.DeadBoidTarget = null;
        _fsm.ChangeState(HunterStateID.Patrol);
    }

    public override void Exit() { }
}
