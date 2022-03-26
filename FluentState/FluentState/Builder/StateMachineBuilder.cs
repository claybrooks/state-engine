namespace FluentState.Builder
{
    public class StateMachineBuilder<TState, TStimulus> 
        where TState : notnull
        where TStimulus : notnull
    {
        private readonly StateMachine<TState, TStimulus> _machine;

        public StateMachineBuilder(TState initialState)
        {
            _machine = new StateMachine<TState, TStimulus>(initialState);
        }

        public StateBuilder<TState, TStimulus> WithState(TState from)
        {
            return new StateBuilder<TState, TStimulus> (this, _machine, from);
        }

        public StateMachineBuilder<TState, TStimulus> WithEnterAction(Action<TState, TState, TStimulus> action)
        {
            _machine.AddStateEnterAction(action);
            return this;
        }

        public StateMachineBuilder<TState, TStimulus> WithLeaveAction(Action<TState, TState, TStimulus> action)
        {
            _machine.AddStateLeaveAction(action);
            return this;
        }

        public StateMachine<TState, TStimulus> Build()
        {
            return _machine;
        }
    }
}
