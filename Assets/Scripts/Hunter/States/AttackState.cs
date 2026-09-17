using UnityEngine;
public class AttackState : IHunterState
{
    public string StateName => "Attack";

    private readonly HunterNPC hunter;
    private readonly HunterStateMachine machine;

    private float attackExecutionTimer;
    private const float AttackExecutionDelay = 0.3f; // pequeño delay para que el ataque se "sienta"
    private bool isExecutingAttack;
    private bool wasInMeleeRange;
    private bool wasInRangeAttack;



    public AttackState(HunterNPC hunter, HunterStateMachine machine)
    {
        this.hunter = hunter;
        this.machine = machine;
    }

    public void Enter()
    {
        isExecutingAttack = false;
        attackExecutionTimer = 0f;
        hunter.SetStateColor(Color.red);
    }

    public void Tick()
    {
        // Si ya está ejecutando el golpe, lo termina sí o sí, sin chequear rango de visión
        if (isExecutingAttack)
        {
            if (hunter.CurrentTarget == null || !hunter.CurrentTarget.gameObject.activeInHierarchy)
            {
                isExecutingAttack = false;
                hunter.CurrentTarget = null;
                machine.ChangeState(new PatrolState(hunter, machine));
                return;
            }

            attackExecutionTimer += Time.deltaTime;
            if (attackExecutionTimer >= AttackExecutionDelay)
            {
                ExecuteHit();
            }
            return;
        }

        // A partir de acá, todavía no comprometió el golpe, por lo que puede abortar por rango
        if (hunter.CurrentTarget == null || !hunter.CurrentTarget.gameObject.activeInHierarchy)
        {
            machine.ChangeState(new PatrolState(hunter, machine));
            return;
        }

        float distToTarget = Vector3.Distance(hunter.transform.position, hunter.CurrentTarget.position);

        if (distToTarget > hunter.VisionRadius)
        {
            hunter.CurrentTarget = null;
            machine.ChangeState(new PatrolState(hunter, machine));
            return;
        }

        if (distToTarget <= hunter.MeleeAttackRadius)
        {
            hunter.Stop();
            BeginAttack(inMelee: true, inRange: true);
        }
        else if (distToTarget <= hunter.RangeAttackRadius)
        {
            hunter.Stop();
            BeginAttack(inMelee: false, inRange: true);
        }
        else
        {
            hunter.MoveTowards(hunter.CurrentTarget.position);
        }
    }

    private void BeginAttack(bool inMelee, bool inRange)
    {
        isExecutingAttack = true;
        attackExecutionTimer = 0f;
        wasInMeleeRange = inMelee;
        wasInRangeAttack = inRange;
    }


    private void ExecuteHit()
    {
        Boid boid = hunter.CurrentTarget != null ? hunter.CurrentTarget.GetComponent<Boid>() : null;

        // El golpe conecta si el objetivo estaba en rango cuando se lanzó el ataque, sin importar cuánto se haya movido durante el breve delay de ejecución.
        if (boid != null && !boid.IsDead && (wasInMeleeRange || wasInRangeAttack))
        {
            boid.Die();
        }

        hunter.AttackTimer = 0f;
        hunter.CurrentTarget = null;
        isExecutingAttack = false;
        machine.ChangeState(new PatrolState(hunter, machine));
    }

    public void Exit() { }
}