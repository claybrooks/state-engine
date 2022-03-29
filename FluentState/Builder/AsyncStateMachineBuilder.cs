namespace FluentState.Builder
{
    public class AsyncStateMachineBuilder<TState, TStimulus> : TStateMachineBuilder<AsyncStateMachine<TState, TStimulus>, TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        public AsyncStateMachineBuilder(TState initialState) : base(initialState)
        {

        }
    }
}
