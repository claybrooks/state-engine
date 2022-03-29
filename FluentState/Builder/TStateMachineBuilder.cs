using System;

namespace FluentState.Builder
{
    public class TStateMachineBuilder<TStateMachine, TState, TStimulus> : IStateMachineBuilder<TStateMachine, TState, TStimulus>
        where TStateMachine : IStateMachine<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        protected readonly TStateMachine _machine;

        public TStateMachineBuilder(TState initialState)
        {
            var maybeMachine = (TStateMachine)Activator.CreateInstance(typeof(TStateMachine), initialState)!;
            if (maybeMachine == null)
            {
                throw new InvalidOperationException("Unable to create state machine");
            }
            _machine = maybeMachine;
        }

        public IStateMachine<TState, TStimulus> Machine => _machine;

        public IStateBuilder<TStateMachine, TState, TStimulus> WithState(TState from)
        {
            return new StateBuilder<TStateMachine, TState, TStimulus> (this, from);
        }

        public IStateMachineBuilder<TStateMachine, TState, TStimulus> WithEnterAction(Action<TState, TState, TStimulus> action)
        {
            _machine.AddStateEnterAction(action);
            return this;
        }

        public IStateMachineBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(Action<TState, TState, TStimulus> action)
        {
            _machine.AddStateLeaveAction(action);
            return this;
        }

        public TStateMachine Build()
        {
            return _machine;
        }

        public IStateMachineBuilder<TStateMachine, TState, TStimulus> WithUnboundedHistory()
        {
            _machine.History.Enabled = true;
            _machine.History.MakeUnbounded();
            return this;
        }

        public IStateMachineBuilder<TStateMachine, TState, TStimulus> WithBoundedHistory(int size)
        {
            _machine.History.Enabled = true;
            _machine.History.MakeBounded(size);
            return this;
        }
    }
}
