using System.Threading.Channels;

namespace StateEngine.Deferred;

public static class BuilderExtensions
{
    public static IDeferredStateEngine<TState, TStimulus> BuildDeferredStateEngine<TState, TStimulus>(this IBuilder<TState, TStimulus> builder)
        where TState : struct
        where TStimulus : struct
    {
        return builder.Build<DeferredStateEngineFactory<TState, TStimulus>>() as IDeferredStateEngine<TState, TStimulus> 
               ?? throw new InvalidCastException($"Unable to cast output from {nameof(DeferredStateEngineFactory<TState, TStimulus>)} to {nameof(IDeferredStateEngine<TState, TStimulus>)}");
    }
}

public interface IDeferredStateEngine<out TState, TStimulus> : IStateEngine<TState, TStimulus>, IDisposable
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
    Task<bool> PostAndWaitAsync(TStimulus stimulus, CancellationToken token = default);

    /// <summary>
    /// Waits for the state machine queue to become empty.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task AwaitIdleAsync(CancellationToken token = default);
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

internal sealed class DeferredStateEngine<TState, TStimulus> : IDeferredStateEngine<TState, TStimulus>
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
    private readonly Nito.AsyncEx.AsyncManualResetEvent _idleResetEvent = new();

    public DeferredStateEngine(IStateEngine<TState, TStimulus> stateEngineImpl, bool waitForEngineIdleOnDispose = true)
    {
        _idleResetEvent.Set();

        _stateEngineImpl = stateEngineImpl;
        _waitForEngineIdleOnDispose = waitForEngineIdleOnDispose;

        _queueProcessingTask = Task.Factory.StartNew(DoHandleQueue).GetAwaiter().GetResult();
    }

    #region Forward To Synchronous Machine

    public bool ThrowExceptionOnFailedTransition { get => _stateEngineImpl.ThrowExceptionOnFailedTransition; set => _stateEngineImpl.ThrowExceptionOnFailedTransition = value; }
    public bool ThrowExceptionOnSameStateTransition { get => _stateEngineImpl.ThrowExceptionOnSameStateTransition; set=> _stateEngineImpl.ThrowExceptionOnSameStateTransition = value; }
    public TState CurrentState => _stateEngineImpl.CurrentState;
    public IEnumerable<IHistoryItem<TState, TStimulus>> History => _stateEngineImpl.History;

    #endregion

    #region Async API

    public async Task<bool> PostAsync(TStimulus stimulus, CancellationToken token = default)
    {
        await _stimulusChannel.Writer.WriteAsync(stimulus, token).AsTask().ContinueWith(t =>
        {
            _idleResetEvent.Reset();
        }).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> PostAndWaitAsync(TStimulus stimulus, CancellationToken token = default)
    {
        var posted = await PostAsync(stimulus, token).ConfigureAwait(false);
        if (!posted)
        {
            return false;
        }
        await AwaitIdleAsync(token).ConfigureAwait(false);
        return true;
    }

    public async Task AwaitIdleAsync(CancellationToken token = default)
    {
        await _idleResetEvent.WaitAsync(token).ConfigureAwait(false);
    }

    #endregion

    #region Dispoable

    public void Dispose()
    {
        if (_waitForEngineIdleOnDispose)
        {
            AwaitIdleAsync().GetAwaiter().GetResult();
        }

        _cancellationTokenSource.Cancel();
        _queueProcessingTask.GetAwaiter().GetResult();
    }

    #endregion

    #region Queue Handling

    private async Task DoHandleQueue()
    {
        try
        {
            await DoProcessQueue(_stimulusChannel.Reader, _cancellationTokenSource.Token).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
        {

        }
    }

    private async Task DoProcessQueue(ChannelReader<TStimulus> reader, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            token.ThrowIfCancellationRequested();

            while (await reader.WaitToReadAsync(token).ConfigureAwait(false))
            {
                while (reader.TryRead(out var next))
                {
                    token.ThrowIfCancellationRequested();
                    await _stateEngineImpl.PostAsync(next, token).ConfigureAwait(false);
                }

                _idleResetEvent.Set();
            }
        }
    }

    #endregion
}