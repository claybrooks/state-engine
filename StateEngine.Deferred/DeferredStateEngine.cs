using System.Threading.Channels;

namespace StateEngine.Deferred;

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

public class DeferredStateEngineBuilder<TState, TStimulus> : Builder<IDeferredStateEngine<TState, TStimulus>, TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public DeferredStateEngineBuilder(TState initialState) : base(initialState, new DeferredStateEngineFactory<TState, TStimulus>())
    {
    }
}

public sealed class DeferredStateEngineFactory<TState, TStimulus> : IStateEngineFactory<IDeferredStateEngine<TState, TStimulus>, TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public IDeferredStateEngine<TState, TStimulus> Create(TState initialState,
        ITransitionActionRegistry<TState, TStimulus> enterActions,
        ITransitionActionRegistry<TState, TStimulus> leaveActions,
        IStateMap<TState, TStimulus> stateTransitions,
        ITransitionGuardRegistry<TState, TStimulus> guardRegistry,
        IHistory<TState, TStimulus> history)
    {
        var state_engine_factory = new StateEngineFactory<TState, TStimulus>();
        return new DeferredStateEngine<TState, TStimulus>(state_engine_factory.Create(initialState, enterActions, leaveActions, stateTransitions, guardRegistry, history));
    }
}

internal sealed class DeferredStateEngine<TState, TStimulus> : IDeferredStateEngine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    // Synchronous machine that handles most of the work
    private readonly IStateEngine<TState, TStimulus> _stateEngine;

    // Queue for holding stimuli
    private readonly Channel<TStimulus> _stimulusChannel = Channel.CreateUnbounded<TStimulus>();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _queueProcessingTask;

    public DeferredStateEngine(IStateEngine<TState, TStimulus> stateEngine)
    {
        _stateEngine = stateEngine;
        _queueProcessingTask = Task.Factory.StartNew(async () =>
        {
            await DoHandleQueue(_cancellationTokenSource.Token);
        }, TaskCreationOptions.LongRunning);
    }

    #region Forward To Synchronous Machine

    public bool ThrowExceptionOnFailedTransition { get => _stateEngine.ThrowExceptionOnFailedTransition; set => _stateEngine.ThrowExceptionOnFailedTransition = value; }
    public bool ThrowExceptionOnSameStateTransition { get => _stateEngine.ThrowExceptionOnSameStateTransition; set=> _stateEngine.ThrowExceptionOnSameStateTransition = value; }
    public TState CurrentState => _stateEngine.CurrentState;
    public IEnumerable<IHistoryItem<TState, TStimulus>> History => _stateEngine.History;

    #endregion

    #region Async API

    public async Task<bool> PostAsync(TStimulus stimulus, CancellationToken token = default)
    {
        await _stimulusChannel.Writer.WriteAsync(stimulus, token);
        return true;
    }

    public async Task<bool> PostAndWaitAsync(TStimulus stimulus, CancellationToken token = default)
    {
        var posted = await PostAsync(stimulus, token);
        if (!posted)
        {
            return false;
        }
        await AwaitIdleAsync(token);
        return true;
    }

    public async Task AwaitIdleAsync(CancellationToken token = default)
    {
        var idler = DoCreateEngineIdleTask(token);
        await idler;
    }

    #endregion
    
    public void Dispose()
    {
        var idler = DoCreateEngineIdleTask();
        idler.Wait();
        _cancellationTokenSource.Cancel();
    }

    private Task DoCreateEngineIdleTask(CancellationToken token = default)
    {
        return Task.Factory.StartNew(async () =>
        {
            while (_stimulusChannel.Reader.Count != 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1), token);
            }
        }, token);
    }

    private async Task DoHandleQueue(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var next = await _stimulusChannel.Reader.ReadAsync(token);
                await _stateEngine.PostAsync(next, token);
            }
            catch (TaskCanceledException)
            {
                // This is ok
            }
        }
    }
}