using UnityEngine;

/// <summary>
/// El Hunter recorre los waypoints y genera POIs periódicamente.
/// Transiciona a Attack si detecta un boid vivo, o a Gather si detecta uno muerto.
/// </summary>
public class PatrolState : IHunterState
{
    public string StateName => "Patrol";

    private readonly HunterNPC hunter;
    private readonly HunterStateMachine machine;

    public PatrolState(HunterNPC hunter, HunterStateMachine machine)
    {
        this.hunter = hunter;
        this.machine = machine;
    }

    public void Enter()
    {
        hunter.PoiSpawnTimer = 0f;
        hunter.SetStateColor(Color.green);
    }

    public void Tick()
    {
        // Generación periódica de objetos de interés
        hunter.PoiSpawnTimer += Time.deltaTime;
        if (hunter.PoiSpawnTimer >= hunter.PoiSpawnInterval)
        {
            hunter.PoiSpawnTimer = 0f;
            hunter.TrySpawnPOI();
        }

        // Transición a Gather: prioridad sobre seguir patrullando si hay un boid muerto cerca
        Boid deadBoid = hunter.GetDeadBoidInVision();
        if (deadBoid != null)
        {
            hunter.DeadBoidTarget = deadBoid;
            machine.ChangeState(new GatherState(hunter, machine));
            return;
        }

        // Transición a Attack: solo si el TBA ya terminó y hay un boid vivo en rango
        if (hunter.AttackTimer >= hunter.TimeBetweenAttacks)
        {
            Transform target = hunter.GetClosestBoidInVision();
            if (target != null)
            {
                hunter.CurrentTarget = target;
                machine.ChangeState(new AttackState(hunter, machine));
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

    public void Exit() { }
}