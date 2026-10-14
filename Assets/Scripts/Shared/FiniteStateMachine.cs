using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine<T>
{
    private State<T> _currentState;
    private readonly Dictionary<T, State<T>> _allStates = new();

    public T CurrentID { get; private set; }

    public void AddState(T ID, State<T> state)
    {
        state.SetFSM(this);
        _allStates[ID] = state;
    }

    public void ChangeState(T ID)
    {
        if (!_allStates.ContainsKey(ID))
        {
            Debug.LogError($"Missing State: {ID}");
            return;
        }

        _currentState?.Exit();
        _currentState = _allStates[ID];
        CurrentID = ID;
        _currentState.Enter();
    }

    public void Update()
    {
        _currentState?.Update();
    }
}
