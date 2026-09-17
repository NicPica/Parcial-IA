using UnityEngine;

/// <summary>
/// El Hunter se dirige hacia un boid eliminado, lo recolecta durante un tiempo
/// determinado, y lo hace desaparecer (lo que dispara su respawn en el BoidSpawner).
/// </summary>
public class GatherState : IHunterState
{
    public string StateName => "Gather";

    private readonly HunterNPC hunter;
    private readonly HunterStateMachine machine;

    private const float GatherDuration = 1.5f;
    private const float GatherStopDistance = 1.2f;

    private float gatherTimer;
    private bool isGathering;

    public GatherState(HunterNPC hunter, HunterStateMachine machine)
    {
        this.hunter = hunter;
        this.machine = machine;
    }

    public void Enter()
    {
        gatherTimer = 0f;
        isGathering = false;
        hunter.SetStateColor(Color.yellow);
    }

    public void Tick()
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
            isGathering = false;
            gatherTimer = 0f;
            hunter.MoveTowards(target.transform.position);
            return;
        }

        // Llegó: ejecuta la acción de recolección durante GatherDuration segundos
        hunter.Stop();
        isGathering = true;
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
        machine.ChangeState(new PatrolState(hunter, machine));
    }

    private void AbandonGather()
    {
        hunter.DeadBoidTarget = null;
        machine.ChangeState(new PatrolState(hunter, machine));
    }

    public void Exit() { }
}