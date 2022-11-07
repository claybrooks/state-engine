namespace StateEngine;

public interface IStateEngine<out TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    /// <summary>
    /// This does not consider failed transitions due to transitionGuardRegistry calls.  Only when there is no destination state based on
    /// <see cref="StateEngine{TState,TStimulus}.CurrentState"/> and the provided <typeparamref name="TStimulus"/>
    /// </summary>
    bool ThrowExceptionOnFailedTransition { get; set; }

    /// <summary>
    /// Should most likely be turned off outside of development.  Helpful in determining possible bugs in your state machine
    /// setup
    /// </summary>
    bool ThrowExceptionOnSameStateTransition { get; set; }
    
    TState CurrentState { get; }

    /// <summary>
    /// Queue's the provided <typeparamref name="TStimulus"/> to the state machine, but defers execution.
    /// </summary>
    /// <param name="stimulus"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> Post(TStimulus stimulus, CancellationToken token = default);
    
    IEnumerable<IHistoryItem<TState, TStimulus>> History { get; }
}

public sealed class StateEngineBuilder<TState, TStimulus> : Builder<IStateEngine<TState, TStimulus>, TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public StateEngineBuilder(TState initialState) : base(initialState, new StateEngineFactory<TState, TStimulus>())
    {
    }
}

public sealed class StateEngineFactory<TState, TStimulus> : IStateEngineFactory<IStateEngine<TState, TStimulus>, TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public IStateEngine<TState, TStimulus> Create(TState initialState, ITransitionActionRegistry<TState, TStimulus> enterActions, ITransitionActionRegistry<TState, TStimulus> leaveActions,
        IStateMap<TState, TStimulus> stateTransitions, ITransitionGuardRegistry<TState, TStimulus> guardRegistry, IHistory<TState, TStimulus> history)
    {
        return new StateEngine<TState, TStimulus>(initialState, enterActions, leaveActions, stateTransitions, guardRegistry, history);
    }
}

internal sealed class StateEngine<TState, TStimulus> : IStateEngine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    // Action registries
    private readonly ITransitionActionRegistry<TState, TStimulus> _enterActions;
    private readonly ITransitionActionRegistry<TState, TStimulus> _leaveActions;

    // Allowed transitions
    private readonly IStateMap<TState, TStimulus> _stateTransitions;

    // Guards for transitions
    private readonly ITransitionGuardRegistry<TState, TStimulus> _guardRegistry;

    // History
    private readonly IHistory<TState, TStimulus> _history;

    public StateEngine(TState initialState,
        ITransitionActionRegistry<TState, TStimulus> enterActions,
        ITransitionActionRegistry<TState, TStimulus> leaveActions,
        IStateMap<TState, TStimulus> stateTransitions,
        ITransitionGuardRegistry<TState, TStimulus> guardRegistry,
        IHistory<TState, TStimulus> history)
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