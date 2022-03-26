using System;
using System.Collections.Generic;

namespace FluentState.Builder
{
    public interface IStateBuilder<TStateMachine, TState, TStimulus>
        where TStateMachine : IStateMachine<TState, TStimulus>
        where TState : notnull
        where TStimulus : notnull
    {
        IStateBuilder<TStateMachine, TState, TStimulus> CanTransitionTo(TState to, TStimulus when, IEnumerable<Action<TState, TState, TStimulus>>? actions = null, Func<TState, TState, TStimulus, bool>? guard = null);
        IStateBuilder<TStateMachine, TState, TStimulus> WithEnterAction(Action<TState, TState, TStimulus> action);
        IStateBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(Action<TState, TState, TStimulus> action);
        IStateBuilder<TStateMachine, TState, TStimulus> WithEnterAction(TState from, TStimulus reason, Action<TState, TState, TStimulus> action);
        IStateBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(TState to, TStimulus reason, Action<TState, TState, TStimulus> action);
        IStateMachineBuilder<TStateMachine, TState, TStimulus> Build();
    }
}
