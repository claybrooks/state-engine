using System;
using System.Collections.Generic;
using FluentState.Builder;
using FluentState.History;
using FluentState.MachineParts;

namespace FluentState.Machine;

public sealed class StateMachineBuilder<TState, TStimulus> : Builder<SynchronousStateMachine<TState, TStimulus>, TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public StateMachineBuilder(TState initialState) : base(initialState, new StateMachineFactory<TState, TStimulus>())
    {
    }
}

public sealed class StateMachineFactory<TState, TStimulus> : IStateMachineFactory<SynchronousStateMachine<TState, TStimulus>, TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public SynchronousStateMachine<TState, TStimulus> Create(TState initialState, IActionRegistry<TState, TStimulus> enterActions, IActionRegistry<TState, TStimulus> leaveActions,
        IStateMap<TState, TStimulus> stateTransitions, IStateGuard<TState, TStimulus> stateGuard, IStateMachineHistory<TState, TStimulus> history)
    {
        return new SynchronousStateMachine<TState, TStimulus>(initialState, enterActions, leaveActions, stateTransitions,
            stateGuard, history);
    }
}

public sealed class SynchronousStateMachine<TState, TStimulus> : ISynchronousStateMachine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    // Action registries
    private readonly IActionRegistry<TState, TStimulus> _enterActions;
    private readonly IActionRegistry<TState, TStimulus> _leaveActions;

    // Allowed transitions
    private readonly IStateMap<TState, TStimulus> _stateTransitions;

    // Guards for transitions
    private readonly IStateGuard<TState, TStimulus> _stateGuard;

    // History
    private readonly IStateMachineHistory<TState, TStimulus> _history;

    public SynchronousStateMachine(TState initialState,
        IActionRegistry<TState, TStimulus> enterActions,
        IActionRegistry<TState, TStimulus> leaveActions,
        IStateMap<TState, TStimulus> stateTransitions,
        IStateGuard<TState, TStimulus> stateGuard,
        IStateMachineHistory<TState, TStimulus> history)
    {
        CurrentState = initialState;
        _enterActions = enterActions;
        _leaveActions = leaveActions;
        _stateTransitions = stateTransitions;
        _stateGuard = stateGuard;
        _history = history;
    }

    public bool ThrowExceptionOnFailedTransition { get; set; } = false;

    public bool ThrowExceptionOnSameStateTransition { get; set; } = false;

    public TState CurrentState { get; private set; }

    public IEnumerable<HistoryItem<TState, TStimulus>> History => _history;

    public void OverrideState(TState state)
    {
        CurrentState = state;
    }

    public bool Post(TStimulus stimulus)
    {
        // Unable to get the next state with the supplied stimulus
        if (!_stateTransitions.CheckTransition(CurrentState, stimulus, out var next_state))
        {
            if (ThrowExceptionOnFailedTransition)
            {
                throw new Exception(
                    $"No state transition available.  Current State: {CurrentState}, Stimulus: {stimulus}");
            }
            return false;
        }

        // The next state is the current state, so no transition
        if (CurrentState.Equals(next_state))
        {
            if (ThrowExceptionOnSameStateTransition)
            {
                throw new Exception($"Trying to transition to same state.  Current State: {CurrentState}, Stimulus: {stimulus}");
            }
            return false;
        }

        var transition = new Transition<TState, TStimulus> { From = CurrentState, To = next_state, Reason = stimulus };

        if (!_stateGuard.CheckTransition(transition))
        {
            return false;
        }

        _leaveActions.Trigger(CurrentState, transition);
        CurrentState = next_state;
        _enterActions.Trigger(CurrentState, transition);

        _history.Add(CurrentState, next_state, stimulus);

        return true;
    }
}