using System;

namespace FluentState.Builder
{
    public interface IStateMachineBuilder<TStateMachine, TState, TStimulus>
        where TStateMachine : IStateMachine<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        IStateMachine<TState, TStimulus> Machine { get; }

        IStateBuilder<TStateMachine, TState, TStimulus> WithState(TState state);
        IStateMachineBuilder<TStateMachine, TState, TStimulus> WithEnterAction(Action<TState, TState, TStimulus> action);
        IStateMachineBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(Action<TState, TState, TStimulus> action);
        IStateMachineBuilder<TStateMachine, TState, TStimulus> WithUnboundedHistory();
        IStateMachineBuilder<TStateMachine, TState, TStimulus> WithBoundedHistory(int size);
        TStateMachine Build();
    }
}
