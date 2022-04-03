using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluentState.Machine
{
    public interface IAsyncStateMachine<TState, TStimulus> : IStateMachine<TState, TStimulus>, IDisposable
        where TState : struct
        where TStimulus : struct
    {
        /// <summary>
        /// Queue's the provided <typeparamref name="TStimulus"/> to the state machine, but defers execution.
        /// </summary>
        /// <remarks>
        /// Callers that care about completion of processing the <typeparamref name="TStimulus"/> should consider
        /// <see cref="PostAndWaitAsync(TStimulus, CancellationToken)"/>, or consider immediately calling <see cref="AwaitIdleAsync(CancellationToken)"/>.
        /// </remarks>
        /// <param name="stimulus"></param>
        /// <returns></returns>
        new bool Post(TStimulus stimulus);

        /// <summary>
        /// Queue's the provided <typeparamref name="TStimulus"/> to the state machine and waits for the machine to go idle.
        /// </summary>
        /// <remarks>
        /// If the state machine has stimuli queued prior to this call, the caller will be implicitly awaiting the completion
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
}
