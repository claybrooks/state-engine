using System;
using System.Collections.Generic;

namespace FluentState.Config
{
    public interface IConfigLoader<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        /// <summary>
        /// Initial state of the state machine
        /// </summary>
        TState InitialState { get; }

        /// <summary>
        /// Actions to be run when any state is entered
        /// </summary>
        IEnumerable<Action<TState, TState, TStimulus>> GlobalEnterActions { get; }
        
        /// <summary>
        /// Actions to be run when any state is left
        /// </summary>
        IEnumerable<Action<TState, TState, TStimulus>> GlobalLeaveActions { get; }

        /// <summary>
        /// List of state configs
        /// </summary>
        IEnumerable<StateConfig<TState, TStimulus>> States { get; }
    }
}
