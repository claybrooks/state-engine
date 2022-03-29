using System.Threading;
using System.Threading.Tasks;

namespace FluentState.Persistence
{
    public interface ISerializer<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        Task<bool> Save(IStateMachine<TState, TStimulus> machine, string path, CancellationToken cancellationToken);
        Task<bool> Load(IStateMachine<TState, TStimulus> machine, string path, CancellationToken cancellationToken);
    }
}
