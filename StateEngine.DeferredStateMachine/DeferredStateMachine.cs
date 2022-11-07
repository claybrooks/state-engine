using System.Threading.Channels;

namespace StateEngine.StateMachine.Deferred;

public sealed class DeferredStateMachineBuilder<TState, TStimulus> : Builder<DeferredStateMachine<TState, TStimulus>, TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public DeferredStateMachineBuilder(TState initialState) : base(initialState, new DeferredStateMachineFactory<TState, TStimulus>())
    {
    }
}

internal sealed class DeferredStateMachineFactory<TState, TStimulus> : IStateMachineFactory<DeferredStateMachine<TState, TStimulus>, TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public DeferredStateMachine<TState, TStimulus> Create(TState initialState, ITransitionActionRegistry<TState, TStimulus> enterActions, ITransitionActionRegistry<TState, TStimulus> leaveActions,
        IStateMap<TState, TStimulus> stateTransitions, ITransitionGuardRegistry<TState, TStimulus> guardRegistry, IStateMachineHistory<TState, TStimulus> history)
    {
        return new DeferredStateMachine<TState, TStimulus>(new StateMachine<TState, TStimulus>(initialState, enterActions, leaveActions, stateTransitions, guardRegistry, history));
    }
}

public sealed class DeferredStateMachine<TState, TStimulus> : IDeferredStateMachine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    // Synchronous machine that handles most of the work
    private readonly IStateMachine<TState, TStimulus> _stateMachine;

    // Queue for holding stimuli
    private readonly Channel<TStimulus> _stimulusChannel = Channel.CreateUnbounded<TStimulus>();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _queueProcessingTask;

    public DeferredStateMachine(IStateMachine<TState, TStimulus> stateMachine)
    {
        _stateMachine = stateMachine;
        _queueProcessingTask = Task.Factory.StartNew(async () =>
        {
            await DoHandleQueue(_cancellationTokenSource.Token);
        }, TaskCreationOptions.LongRunning);
    }

    #region Forward To Synchronous Machine

    public bool ThrowExceptionOnFailedTransition { get => _stateMachine.ThrowExceptionOnFailedTransition; set => _stateMachine.ThrowExceptionOnFailedTransition = value; }
    public bool ThrowExceptionOnSameStateTransition { get => _stateMachine.ThrowExceptionOnSameStateTransition; set=> _stateMachine.ThrowExceptionOnSameStateTransition = value; }
    public TState CurrentState => _stateMachine.CurrentState;
    public IEnumerable<IHistoryItem<TState, TStimulus>> History => _stateMachine.History;

    #endregion

    #region Async API
    
    public async Task<bool> Post(TStimulus stimulus, CancellationToken token = default)
    {
        await _stimulusChannel.Writer.WriteAsync(stimulus, token);
        return true;
    }

    public async Task<bool> PostAndWaitAsync(TStimulus stimulus, CancellationToken token = default)
    {
        var posted = await Post(stimulus, token);
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
                await _stateMachine.Post(next, token);
            }
            catch (TaskCanceledException)
            {
                // This is ok
            }
        }
    }
}