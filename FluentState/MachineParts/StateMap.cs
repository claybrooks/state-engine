using System.Collections.Generic;
using System.Linq;

namespace FluentState.MachineParts;

public interface IStateMap<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    bool Register(Transition<TState, TStimulus> transition);
    bool CheckTransition(TState currentState, TStimulus reason, out TState nextState);
}

public interface IStateMapValidation<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    bool HasTopLevelState(TState state);
    IReadOnlyList<TState> TopLevelStates { get; }
    IReadOnlyDictionary<TStimulus, TState> StateTransitions(TState state);
    bool IsTransitionRegistered(Transition<TState, TStimulus> transition);
}

public class StateMap<TState, TStimulus> : IStateMap<TState, TStimulus>, IStateMapValidation<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
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

    public bool HasTopLevelState(TState state)
    {
        return _stateTransitions.ContainsKey(state);
    }

    public IReadOnlyList<TState> TopLevelStates => _stateTransitions.Keys.ToList();

    public IReadOnlyDictionary<TStimulus, TState> StateTransitions(TState state) => _stateTransitions[state];
    public bool IsTransitionRegistered(Transition<TState, TStimulus> transition)
    {
        if (_stateTransitions.TryGetValue(transition.From, out var value))
        {
            if (value.TryGetValue(transition.Reason, out var state))
            {
                return transition.To.Equals(state);
            }
        }

        return false;
    }
}