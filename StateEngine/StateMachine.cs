namespace StateEngine;

public interface IStateMachineFactory<out TStateMachine, TState, TStimulus>
    where TStateMachine : IStateMachine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    TStateMachine Create(TState initialState,
        ITransitionActionRegistry<TState, TStimulus> enterActions,
        ITransitionActionRegistry<TState, TStimulus> leaveActions,
        IStateMap<TState, TStimulus> stateTransitions,
        ITransitionGuardRegistry<TState, TStimulus> guardRegistry,
        IStateMachineHistory<TState, TStimulus> history);
}

public interface IStateMachine<out TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    /// <summary>
    /// This does not consider failed transitions due to transitionGuardRegistry calls.  Only when there is no destination state based on
    /// <see cref="StateMachine{TState,TStimulus}.CurrentState"/> and the provided <typeparamref name="TStimulus"/>
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

public interface IDeferredStateMachine<out TState, TStimulus> : IStateMachine<TState, TStimulus>, IDisposable
    where TState : struct
    where TStimulus : struct
{
    /// <summary>
    /// Queue's the provided <typeparamref name="TStimulus"/> to the state machine and waits for the machine to go idle.
    /// </summary>
    /// <remarks>
    /// If the state machine has stimuli queued prior to this call, the caller will also be awaiting the completion
    /// of those stimuli.
    /// </remarks>
    /// <param name="stimulus"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> PostAndWaitAsync(TStimulus stimulus, CancellationToken token);

    /// <summary>
    /// Waits for the state machine queue to become empty.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task AwaitIdleAsync(CancellationToken token);
}