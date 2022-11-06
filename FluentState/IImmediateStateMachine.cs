using System.Threading.Tasks;

namespace FluentState;

public interface IImmediateStateMachine<out TState, TStimulus> : IStateMachine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
}