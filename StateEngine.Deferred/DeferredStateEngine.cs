using System.Threading.Channels;

namespace StateEngine.Deferred;

public static class BuilderExtensions
{
    public static IStateEngine<TState, TStimulus> BuildDeferredStateEngine<TState, TStimulus>(this IBuilder<TState, TStimulus> builder)
        where TState : struct
        where TStimulus : struct
    {
        return builder.Build<DeferredStateEngineFactory<TState, TStimulus>>();
    }
}

public sealed class DeferredStateEngineFactory<TState, TStimulus> : IStateEngineFactory<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public IStateEngine<TState, TStimulus> Create(TState initialState,
        ITransitionActionRegistry<TState, TStimulus> enterActions,
        ITransitionActionRegistry<TState, TStimulus> leaveActions,
        IStateMap<TState, TStimulus> stateTransitions,
        ITransitionGuardRegistry<TState, TStimulus> guardRegistry,
        IHistory<TState, TStimulus> history)
    {
        var state_engine_factory = new StateEngineFactory<TState, TStimulus>();
        var state_engine = state_engine_factory.Create(initialState, enterActions, leaveActions, stateTransitions, guardRegistry, history);
        return new DeferredStateEngine<TState, TStimulus>(state_engine);
    }
}

internal sealed class DeferredStateEngine<TState, TStimulus> : IStateEngine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    // Synchronous machine that handles most of the work
    private readonly IStateEngine<TState, TStimulus> _stateEngineImpl;
    private readonly bool _waitForEngineIdleOnDispose;

    // Queue for holding stimuli
    private readonly Channel<TStimulus> _stimulusChannel = Channel.CreateUnbounded<TStimulus>();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _queueProcessingTask;

    public DeferredStateEngine(IStateEngine<TState, TStimulus> stateEngineImpl, bool waitForEngineIdleOnDispose = true)
    {
        _stateEngineImpl = stateEngineImpl;
        _waitForEngineIdleOnDispose = waitForEngineIdleOnDispose;

        _queueProcessingTask = Task.Factory.StartNew(DoHandleQueue).GetAwaiter().GetResult();
    }


    public bool ThrowExceptionOnFailedTransition
    {
        get => _stateEngineImpl.ThrowExceptionOnFailedTransition;
        set => _stateEngineImpl.ThrowExceptionOnFailedTransition = value;
    }

    public bool ThrowExceptionOnSameStateTransition
    {
        get => _stateEngineImpl.ThrowExceptionOnSameStateTransition;
        set=> _stateEngineImpl.ThrowExceptionOnSameStateTransition = value;
    }

    public TState CurrentState => _stateEngineImpl.CurrentState;

    public IEnumerable<IHistoryItem<TState, TStimulus>> History => _stateEngineImpl.History;

    public async Task<bool> PostAsync(TStimulus stimulus, CancellationToken token = default)
    {
        await _stimulusChannel.Writer.WriteAsync(stimulus, token).ConfigureAwait(false);
        return true;
    }

    #region IDisposable

    public void Dispose()
    {
        if (_waitForEngineIdleOnDispose)
        {
            _stimulusChannel.Writer.Complete();
        }
        else
        {
            _cancellationTokenSource.Cancel();
        }

        _queueProcessingTask.GetAwaiter().GetResult();
    }

    #endregion

    #region Queue Handling

    private async Task DoHandleQueue()
    {
        try
        {
            while (await _stimulusChannel.Reader.WaitToReadAsync(_cancellationTokenSource.Token).ConfigureAwait(false))
            {
                while (_stimulusChannel.Reader.TryRead(out var stimulus))
                {
                    await _stateEngineImpl.PostAsync(stimulus, _cancellationTokenSource.Token).ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
        {

        }
    }

    #endregion
}