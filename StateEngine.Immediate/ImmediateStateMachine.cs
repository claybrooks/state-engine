namespace StateEngine.Immediate;

public sealed class ImmediateStateMachineBuilder<TState, TStimulus> : AbstractBuilder<ImmediateStateMachine<TState, TStimulus>, TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public ImmediateStateMachineBuilder(TState initialState) : base(initialState, new ImmediateStateMachineFactory<TState, TStimulus>())
    {
    }
}

internal sealed class ImmediateStateMachineFactory<TState, TStimulus> : IStateMachineFactory<ImmediateStateMachine<TState, TStimulus>, TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public ImmediateStateMachine<TState, TStimulus> Create(TState initialState, IActionRegistry<TState, TStimulus> enterActions, IActionRegistry<TState, TStimulus> leaveActions,
        IStateMap<TState, TStimulus> stateTransitions, IGuardRegistry<TState, TStimulus> guardRegistry, IStateMachineHistory<TState, TStimulus> history)
    {
        return new ImmediateStateMachine<TState, TStimulus>(initialState, enterActions, leaveActions, stateTransitions,
            guardRegistry, history);
    }
}

public sealed class ImmediateStateMachine<TState, TStimulus> : IImmediateStateMachine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    // Action registries
    private readonly IActionRegistry<TState, TStimulus> _enterActions;
    private readonly IActionRegistry<TState, TStimulus> _leaveActions;

    // Allowed transitions
    private readonly IStateMap<TState, TStimulus> _stateTransitions;

    // Guards for transitions
    private readonly IGuardRegistry<TState, TStimulus> _guardRegistry;

    // History
    private readonly IStateMachineHistory<TState, TStimulus> _history;

    public ImmediateStateMachine(TState initialState,
        IActionRegistry<TState, TStimulus> enterActions,
        IActionRegistry<TState, TStimulus> leaveActions,
        IStateMap<TState, TStimulus> stateTransitions,
        IGuardRegistry<TState, TStimulus> guardRegistry,
        IStateMachineHistory<TState, TStimulus> history)
    {
        CurrentState = initialState;
        _enterActions = enterActions;
        _leaveActions = leaveActions;
        _stateTransitions = stateTransitions;
        _guardRegistry = guardRegistry;
        _history = history;
    }

    public bool ThrowExceptionOnFailedTransition { get; set; } = false;

    public bool ThrowExceptionOnSameStateTransition { get; set; } = false;

    public TState CurrentState { get; private set; }

    public IEnumerable<IHistoryItem<TState, TStimulus>> History => _history;

    public async Task<bool> Post(TStimulus stimulus, CancellationToken cancellationToken = default)
    {
        // Unable to get the next state with the supplied stimulus
        if (!_stateTransitions.CheckTransition(CurrentState, stimulus, out var next_state))
        {
            if (ThrowExceptionOnFailedTransition)
            {
                throw new Exception($"No state transition available.  Current State: {CurrentState}, Stimulus: {stimulus}");
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

        if (! await _guardRegistry.CheckTransition(transition))
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