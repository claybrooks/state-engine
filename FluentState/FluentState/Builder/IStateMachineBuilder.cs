namespace FluentState.Builder
{
    public interface IStateMachineBuilder<TStateMachine, TState, TStimulus>
        where TStateMachine : IStateMachine<TState, TStimulus>
        where TState : notnull
        where TStimulus : notnull
    {
        IStateBuilder<TStateMachine, TState, TStimulus> WithState(TState from);
        IStateMachineBuilder<TStateMachine, TState, TStimulus> WithEnterAction(Action<TState, TState, TStimulus> action);
        IStateMachineBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(Action<TState, TState, TStimulus> action);
        TStateMachine Build();
    }
}
