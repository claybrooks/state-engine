using System.Threading;
using System.Threading.Tasks;

namespace FluentState.Persistence
{
    public interface IStateMachineSerializer<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        /// <summary>
        /// Save a <see cref="IStateMachine{TState, TStimulus}"/> captured history.
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> Save(IStateMachine<TState, TStimulus> machine, string path, CancellationToken cancellationToken);

        /// <summary>
        /// Load previously captured history into the provided <paramref name="machine"/>
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> Load(IStateMachine<TState, TStimulus> machine, string path, CancellationToken cancellationToken);
    }
}
