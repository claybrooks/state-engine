using System;
using System.Collections.Generic;

namespace FluentState.Builder
{
    public interface IStateBuilder<TStateMachine, TState, TStimulus>
        where TStateMachine : IStateMachine<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        IStateBuilder<TStateMachine, TState, TStimulus> CanTransitionTo(
            TState to,
            TStimulus when,
            IEnumerable<Action<TState, TState, TStimulus>>? actions = null,
            IEnumerable<Func<TState, TState, TStimulus, bool>>? guards = null
        );

        IStateBuilder<TStateMachine, TState, TStimulus> WithEnterAction(Action<TState, TState, TStimulus> action);
        IStateBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(Action<TState, TState, TStimulus> action);
        IStateBuilder<TStateMachine, TState, TStimulus> WithEnterAction(TState enteringState, TStimulus reason, Action<TState, TState, TStimulus> action);
        IStateBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(TState leavingState, TStimulus reason, Action<TState, TState, TStimulus> action);
        IStateMachineBuilder<TStateMachine, TState, TStimulus> Build();
    }
}
