using System;

namespace FluentState.Config;

public interface IGuardProvider<in TState, in TStimulus>
    where TState : struct
    where TStimulus : struct
{
    /// <summary>
    /// Gets the guard associated with the provided <paramref name="key"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Func<TState, TState, TStimulus, bool> Get(string key);
}