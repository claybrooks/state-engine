using System;
using System.Threading.Tasks;

namespace FluentState
{
    public interface IAsyncStateMachine<TState, TStimulus> : IStateMachine<TState, TStimulus>, IDisposable
        where TState : notnull
        where TStimulus : notnull
    {
        new Task<bool> Post(TStimulus stimulus);
        Task<bool> PostAndWaitAsync(TStimulus stimulus);
        Task AwaitIdleAsync();
    }
}
