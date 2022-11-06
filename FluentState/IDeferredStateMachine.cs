using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluentState;

public interface IDeferredStateMachine<out TState, TStimulus> : IStateMachine<TState, TStimulus>, IAsyncDisposable
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