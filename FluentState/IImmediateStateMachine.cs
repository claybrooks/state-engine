namespace FluentState;

public interface IImmediateStateMachine<TState, TStimulus> : IStateMachine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    /// <summary>
    /// Injects a <see cref="TStimulus"/> into the machine
    /// </summary>
    /// <param name="stimulus"></param>
    /// <returns></returns>
    bool Post(TStimulus stimulus);
}