namespace FluentState.Builder
{
    public class StateBuilder<TState, TStimulus> 
        where TState : notnull
        where TStimulus : notnull
    {
        private readonly StateMachineBuilder<TState, TStimulus> _machineBuilder;
        private readonly StateMachine<TState, TStimulus> _machine;
        private readonly TState _state;

        public StateBuilder(StateMachineBuilder<TState, TStimulus> machineBuilder, StateMachine<TState, TStimulus> machine, TState state)
        {
            _machineBuilder = machineBuilder;
            _machine = machine;
            _state = state;

        }

        public StateBuilder<TState, TStimulus> CanTransitionTo(TState to, TStimulus when, Action<TState, TState, TStimulus>? action = null)
        {
            _machine.AddTransition(_state, to, when);
            if (action != null)
            {
                _machine.AddStateEnterAction(to, _state, when, action);
            }
            return this;
        }

        public StateBuilder<TState, TStimulus> WithEnterAction(Action<TState, TState, TStimulus> action)
        {
            _machine.AddStateEnterAction(_state, action);
            return this;
        }

        public StateBuilder<TState, TStimulus> WithLeaveAction(Action<TState, TState, TStimulus> action)
        {
            _machine.AddStateLeaveAction(_state, action);
            return this;
        }

        public StateBuilder<TState, TStimulus> WithEnterAction(TState from, TStimulus reason, Action<TState, TState, TStimulus> action)
        {
            _machine.AddStateEnterAction(_state, from, reason, action);
            return this;
        }

        public StateBuilder<TState, TStimulus> WithLeaveAction(TState to, TStimulus reason, Action<TState, TState, TStimulus> action)
        {
            _machine.AddStateLeaveAction(_state, to, reason, action);
            return this;
        }

        public StateMachineBuilder<TState, TStimulus> Build()
        {
            return _machineBuilder;
        }
    }
}
