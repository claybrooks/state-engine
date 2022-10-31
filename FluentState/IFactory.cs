using FluentState.History;
using FluentState.MachineParts;

namespace FluentState;

public interface IStateMachineFactory<out TStateMachine, TState, TStimulus>
    where TStateMachine : IStateMachine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    TStateMachine Create(TState initialState,
        IActionRegistry<TState, TStimulus> enterActions,
        IActionRegistry<TState, TStimulus> leaveActions,
        IStateMap<TState, TStimulus> stateTransitions,
        IStateGuard<TState, TStimulus> stateGuard,
        IStateMachineHistory<TState, TStimulus> history);
}