public interface IHunterState
{
    void Enter();
    void Tick();
    void Exit();
    string StateName { get; }
}