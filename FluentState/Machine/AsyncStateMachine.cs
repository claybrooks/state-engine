using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using FluentState.Builder;
using FluentState.History;
using FluentState.MachineParts;

namespace FluentState.Machine;

public sealed class AsyncStateMachineBuilder<TState, TStimulus> : Builder<AsyncStateMachine<TState, TStimulus>, TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public AsyncStateMachineBuilder(TState initialState) : base(initialState, new AsyncStateMachineFactory<TState, TStimulus>())
    {
    }
}

public sealed class AsyncStateMachineFactory<TState, TStimulus> : IStateMachineFactory<AsyncStateMachine<TState, TStimulus>, TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public AsyncStateMachine<TState, TStimulus> Create(TState initialState, IActionRegistry<TState, TStimulus> enterActions, IActionRegistry<TState, TStimulus> leaveActions,
        IStateMap<TState, TStimulus> stateTransitions, IStateGuard<TState, TStimulus> stateGuard, IStateMachineHistory<TState, TStimulus> history)
    {
        return new AsyncStateMachine<TState, TStimulus>(new StateMachine<TState, TStimulus>(initialState, enterActions, leaveActions, stateTransitions, stateGuard, history));
    }
}

public sealed class AsyncStateMachine<TState, TStimulus> : IAsyncStateMachine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    // Synchronous machine that handles most of the work
    private readonly IStateMachine<TState, TStimulus> _stateMachine;

    // Queue for holding stimuli
    private readonly Channel<TStimulus> _stimulusChannel = Channel.CreateUnbounded<TStimulus>();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _queueProcessingTask;

    public AsyncStateMachine(IStateMachine<TState, TStimulus> stateMachine)
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
    public IEnumerable<HistoryItem<TState, TStimulus>> History => _stateMachine.History;
    public void OverrideState(TState state) => _stateMachine.OverrideState(state);

    #endregion

    // This is hidden because it has little meaning to the async intent of this class.  It would bypass the queue and we don't want that
    bool IStateMachine<TState, TStimulus>.Post(TStimulus stimulus)
    {
        throw new NotImplementedException("This function cannot be called.  Call \"PostAndWaitAsync\" instead");
    }

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
        var idler = Task.Factory.StartNew(async () =>
        {
            while (_stimulusChannel.Reader.Count != 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1), token);
            }
        }, token);
        await idler;
    }

    #endregion

    /// <summary>
    /// Will wait for the queue to empty
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        await AwaitIdleAsync();
        _cancellationTokenSource.Cancel();
        await _queueProcessingTask;
    }

    private async Task DoHandleQueue(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var next = await _stimulusChannel.Reader.ReadAsync(token);
                _stateMachine.Post(next);
            }
            catch (TaskCanceledException)
            {
                // This is ok
            }
        }
    }
}