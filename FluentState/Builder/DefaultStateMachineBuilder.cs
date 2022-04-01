namespace FluentState.Builder
{
    public class DefaultStateMachineBuilder<TState, TStimulus> : TStateMachineBuilder<StateMachine<TState, TStimulus>, TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        public DefaultStateMachineBuilder(TState initialState) : base(initialState)
        {

        }
    }
}
