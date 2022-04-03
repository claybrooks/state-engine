using System;

namespace FluentState.Config
{
    public interface IActionProvider<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        /// <summary>
        /// Gets the action associated with the provided <paramref name="key"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Action<TState, TState, TStimulus> Get(string key);
    }
}
