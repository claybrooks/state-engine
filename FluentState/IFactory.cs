using FluentState.Builder;
using FluentState.History;
using FluentState.MachineParts;
using System;

namespace FluentState;

public interface IStateMachineFactory<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    IStateMachine<TState, TStimulus> Create(TState initialState,
        IActionRegistry<TState, TStimulus> enterActions,
        IActionRegistry<TState, TStimulus> leaveActions,
        IStateMap<TState, TStimulus> stateTransitions,
        IStateGuard<TState, TStimulus> stateGuard,
        IStateMachineHistory<TState, TStimulus> history);
}