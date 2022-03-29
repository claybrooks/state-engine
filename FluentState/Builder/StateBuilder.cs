using System;
using System.Collections.Generic;

namespace FluentState.Builder
{
    public class StateBuilder<TStateMachine, TState, TStimulus> : IStateBuilder<TStateMachine, TState, TStimulus>
        where TStateMachine : IStateMachine<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        private readonly IStateMachineBuilder<TStateMachine, TState, TStimulus> _machineBuilder;
        private readonly TState _state;

        public StateBuilder(IStateMachineBuilder<TStateMachine, TState, TStimulus> machineBuilder, TState state)
        {
            _machineBuilder = machineBuilder;
            _state = state;

        }

        public IStateBuilder<TStateMachine, TState, TStimulus> CanTransitionTo(
            TState transitionTo,
            TStimulus when,
            IEnumerable<Action<TState, TState, TStimulus>>? actions = null,
            IEnumerable<Func<TState, TState, TStimulus, bool>>? guards = null
        )
        {
            var machine = _machineBuilder.Machine;

            machine.AddTransition(transitionTo, _state, when);
            if (actions != null)
            {
                foreach(var action in actions)
                {
                    machine.AddStateEnterAction(transitionTo, _state, when, action);
                }
            }

            if (guards != null)
            {
                foreach (var guard in guards)
                {
                    machine.AddTransitionGuard(transitionTo, _state, when, guard);
                }
            }

            return this;
        }

        public IStateBuilder<TStateMachine, TState, TStimulus> WithEnterAction(Action<TState, TState, TStimulus> action)
        {
            _machineBuilder.Machine.AddStateEnterAction(_state, action);
            return this;
        }

        public IStateBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(Action<TState, TState, TStimulus> action)
        {
            _machineBuilder.Machine.AddStateLeaveAction(_state, action);
            return this;
        }

        public IStateBuilder<TStateMachine, TState, TStimulus> WithEnterAction(TState from, TStimulus reason, Action<TState, TState, TStimulus> action)
        {
            _machineBuilder.Machine.AddStateEnterAction(_state, from, reason, action);
            return this;
        }

        public IStateBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(TState to, TStimulus reason, Action<TState, TState, TStimulus> action)
        {
            _machineBuilder.Machine.AddStateLeaveAction(to, _state, reason, action);
            return this;
        }

        public IStateMachineBuilder<TStateMachine, TState, TStimulus> Build()
        {
            return _machineBuilder;
        }
    }
}
