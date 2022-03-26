namespace FluentState.Builder
{
    public class TStateMachineBuilder<TStateMachine, TState, TStimulus> : IStateMachineBuilder<TStateMachine, TState, TStimulus>
        where TStateMachine : IStateMachine<TState, TStimulus>
        where TState : notnull
        where TStimulus : notnull
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

        public IStateBuilder<TStateMachine, TState, TStimulus> WithState(TState from)
        {
            return new StateBuilder<TStateMachine, TState, TStimulus> (this, _machine, from);
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
    }
}
