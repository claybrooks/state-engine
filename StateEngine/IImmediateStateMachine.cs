namespace StateEngine;

public interface IImmediateStateMachine<out TState, TStimulus> : IStateMachine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
}