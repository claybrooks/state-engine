namespace StateEngine.Tests.Stubs;

public class StubEngine<TState, TStimulus> : IStateEngine<TState, TStimulus> where TState : struct where TStimulus : struct
{
    public StubEngine(TState initialState, ITransitionActionRegistry<TState, TStimulus> enterActions, ITransitionActionRegistry<TState, TStimulus> leaveActions,
        IStateMap<TState, TStimulus> stateTransitions, ITransitionGuardRegistry<TState, TStimulus> guardRegistry, IHistory<TState, TStimulus> history)
    {
        CurrentState = initialState;
        EnterActions = enterActions;
        LeaveActions = leaveActions;
        StateTransitions = stateTransitions;
        GuardRegistry = guardRegistry;
        History = history;
    }

    public List<(TState state, TStimulus? stimulus)> PostCalls = new();

    public bool ThrowExceptionOnFailedTransition { get; set; } = false;
    public bool ThrowExceptionOnSameStateTransition { get; set; } = false;

    public TState CurrentState { get; }
    public ITransitionActionRegistry<TState, TStimulus> EnterActions { get; }
    public ITransitionActionRegistry<TState, TStimulus> LeaveActions { get; }
    public IStateMap<TState, TStimulus> StateTransitions { get; }
    public ITransitionGuardRegistry<TState, TStimulus> GuardRegistry { get; }

    public Task OverrideStateAsync(TState state, CancellationToken token)
    {
        PostCalls.Add((state, null));
        return Task.CompletedTask;
    }

    public Task<bool> PostAsync(TStimulus stimulus, CancellationToken token = default)
    {
        PostCalls.Add((CurrentState, stimulus));
        return Task.FromResult(true);
    }

    public IEnumerable<IHistoryItem<TState, TStimulus>> History { get; }

    #region IDisposable

    public void Dispose()
    {
    }

    #endregion
}