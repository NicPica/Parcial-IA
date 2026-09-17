/// <summary>
/// Gestiona el estado actual del Hunter y las transiciones entre estados.
/// Las transiciones SIEMPRE pasan por ChangeState — nunca lógica externa a la FSM.
/// </summary>
public class HunterStateMachine
{
    public IHunterState CurrentState { get; private set; }

    public void ChangeState(IHunterState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Tick()
    {
        CurrentState?.Tick();
    }
}