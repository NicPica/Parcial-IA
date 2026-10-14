/// <summary>
/// Identificadores de los estados de la FSM del NPC Cazador.
/// Se usan como clave de registro en la FiniteStateMachine y para pedir transiciones.
/// </summary>
public enum HunterStateID
{
    Patrol,
    Attack,
    Gather
}

/// <summary>
/// Base común de los estados del Cazador.
/// Hereda de State&lt;HunterStateID&gt; (la clase de la cátedra) y guarda la referencia
/// al NPC, que es la única dependencia compartida por Patrol, Attack y Gather.
/// </summary>
public abstract class HunterState : State<HunterStateID>
{
    protected readonly HunterNPC hunter;

    protected HunterState(HunterNPC hunter)
    {
        this.hunter = hunter;
    }
}
