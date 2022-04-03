using FluentState.Machine;

namespace FluentState.Builder
{
    public class StateMachineBuilder<TState, TStimulus> : TStateMachineBuilder<StateMachine<TState, TStimulus>, TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        public StateMachineBuilder(TState initialState) : base(initialState)
        {

        }
    }
}
