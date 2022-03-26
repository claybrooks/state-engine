using System;
using System.Collections.Generic;

namespace FluentState.Builder
{
    public class StateBuilder<TStateMachine, TState, TStimulus> : IStateBuilder<TStateMachine, TState, TStimulus>
        where TStateMachine : IStateMachine<TState, TStimulus>
        where TState : notnull
        where TStimulus : notnull
    {
        private readonly IStateMachineBuilder<TStateMachine, TState, TStimulus> _machineBuilder;
        private readonly IStateMachine<TState, TStimulus> _machine;
        private readonly TState _state;

        public StateBuilder(IStateMachineBuilder<TStateMachine, TState, TStimulus> machineBuilder, IStateMachine<TState, TStimulus> machine, TState state)
        {
            _machineBuilder = machineBuilder;
            _machine = machine;
            _state = state;

        }

        public IStateBuilder<TStateMachine, TState, TStimulus> CanTransitionTo(
            TState to,
            TStimulus when,
            IEnumerable<Action<TState, TState, TStimulus>>? actions = null,
            IEnumerable<Func<TState, TState, TStimulus, bool>>? guards = null
        )
        {
            _machine.AddTransition(_state, to, when);
            if (actions != null)
            {
                foreach(var action in actions)
                {
                    _machine.AddStateEnterAction(to, _state, when, action);
                }
            }

            if (guards != null)
            {
                foreach (var guard in guards)
                {
                    _machine.AddTransitionGuard(_state, to, when, guard);
                }
            }

            return this;
        }

        public IStateBuilder<TStateMachine, TState, TStimulus> WithEnterAction(Action<TState, TState, TStimulus> action)
        {
            _machine.AddStateEnterAction(_state, action);
            return this;
        }

        public IStateBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(Action<TState, TState, TStimulus> action)
        {
            _machine.AddStateLeaveAction(_state, action);
            return this;
        }

        public IStateBuilder<TStateMachine, TState, TStimulus> WithEnterAction(TState from, TStimulus reason, Action<TState, TState, TStimulus> action)
        {
            _machine.AddStateEnterAction(_state, from, reason, action);
            return this;
        }

        public IStateBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(TState to, TStimulus reason, Action<TState, TState, TStimulus> action)
        {
            _machine.AddStateLeaveAction(_state, to, reason, action);
            return this;
        }

        public IStateMachineBuilder<TStateMachine, TState, TStimulus> Build()
        {
            return _machineBuilder;
        }
    }
}
