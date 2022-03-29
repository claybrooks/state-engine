using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluentState
{
    public interface IAsyncStateMachine<TState, TStimulus> : IStateMachine<TState, TStimulus>, IDisposable
        where TState : struct
        where TStimulus : struct
    {
        new bool Post(TStimulus stimulus);
        Task<bool> PostAndWaitAsync(TStimulus stimulus, CancellationToken token);
        Task AwaitIdleAsync(CancellationToken token);
    }
}
