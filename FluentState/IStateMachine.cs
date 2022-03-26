using System;

namespace FluentState
{
    public interface IStateMachine<TState, TStimulus>
        where TState : notnull
        where TStimulus : notnull
    {
        public TState CurrentState { get; }

        void OverrideState(TState state);

        bool AddTransitionGuard(TState fromState, TState toState, TStimulus when, Func<TState, TState, TStimulus, bool> guard);

        bool AddTransition(TState fromState, TState toState, TStimulus when);

        void AddStateEnterAction(Action<TState, TState, TStimulus> action);

        void AddStateEnterAction(TState state, Action<TState, TState, TStimulus> action);

        void AddStateEnterAction(TState enteringState, TState leavingState, TStimulus reason, Action<TState, TState, TStimulus> action);

        void AddStateLeaveAction(Action<TState, TState, TStimulus> action);

        void AddStateLeaveAction(TState state, Action<TState, TState, TStimulus> action);

        void AddStateLeaveAction(TState leavingState, TState enteringState, TStimulus reason, Action<TState, TState, TStimulus> action);

        bool Post(TStimulus stimulus);
    }
}
