namespace FluentState.Builder
{
    public class AsyncStateMachineBuilder<TState, TStimulus> : TStateMachineBuilder<AsyncStateMachine<TState, TStimulus>, TState, TStimulus>
        where TState : notnull
        where TStimulus : notnull
    {
        public AsyncStateMachineBuilder(TState initialState) : base(initialState)
        {

        }
    }
}
