using System.Collections.Generic;

namespace FluentState.MachineParts;

public interface IStateMap<TState, TStimulus> where TState : struct where TStimulus : struct
{
    bool Register(Transition<TState, TStimulus> transition);
    bool CheckTransition(TState currentState, TStimulus reason, out TState nextState);
}

public class StateMap<TState, TStimulus> : IStateMap<TState, TStimulus> where TState : struct where TStimulus : struct
{
    private readonly Dictionary<TState, Dictionary<TStimulus, TState>> _stateTransitions = new();

    public bool Register(Transition<TState, TStimulus> transition)
    {
        if (!_stateTransitions.ContainsKey(transition.From))
        {
            _stateTransitions.Add(transition.From, new Dictionary<TStimulus, TState>());
        }

        return _stateTransitions[transition.From].TryAdd(transition.Reason, transition.To);
    }

    public bool CheckTransition(TState currentState, TStimulus reason, out TState nextState)
    {
        nextState = currentState;

        if (!_stateTransitions.TryGetValue(currentState, out var state_stimuli) || !state_stimuli.TryGetValue(reason, out var state))
        {
            return false;
        }

        nextState = state;
        return true;
    }
}