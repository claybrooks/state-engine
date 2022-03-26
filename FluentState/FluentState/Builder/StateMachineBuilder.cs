namespace FluentState.Builder
{
    public class StateMachineBuilder<TState, TStimulus> : TStateMachineBuilder<StateMachine<TState, TStimulus>, TState, TStimulus>
        where TState : notnull
        where TStimulus : notnull
    {
        public StateMachineBuilder(TState initialState) : base(initialState)
        {

        }
    }
}
