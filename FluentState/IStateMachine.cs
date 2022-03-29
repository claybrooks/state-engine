using FluentState.History;
using System;

namespace FluentState
{
    public interface IStateMachine<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        TState CurrentState { get; }

        IStateMachineHistory<TState, TStimulus> History { get; }

        void OverrideState(TState state);

        bool AddTransitionGuard(TState enteringState, TState leavingState, TStimulus when, Func<TState, TState, TStimulus, bool> guard);

        bool AddTransition(TState enteringState, TState leavingState, TStimulus when);

        void AddStateEnterAction(Action<TState, TState, TStimulus> action);
        void AddStateEnterAction(TState state, Action<TState, TState, TStimulus> action);
        void AddStateEnterAction(TState enteringState, TState leavingState, TStimulus reason, Action<TState, TState, TStimulus> action);

        void AddStateLeaveAction(Action<TState, TState, TStimulus> action);
        void AddStateLeaveAction(TState state, Action<TState, TState, TStimulus> action);
        void AddStateLeaveAction(TState enteringState, TState leavingState, TStimulus reason, Action<TState, TState, TStimulus> action);

        bool Post(TStimulus stimulus);
    }
}
