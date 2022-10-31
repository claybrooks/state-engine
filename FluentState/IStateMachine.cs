using FluentState.History;
using System.Collections.Generic;
using FluentState.Machine;

namespace FluentState;

public interface IStateMachine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    /// <summary>
    /// This does not consider failed transitions do to guard calls.  Only when there is no destination state based on
    /// <see cref="StateMachine{TState,TStimulus}.CurrentState"/> and the provided stimulus
    /// </summary>
    bool ThrowExceptionOnFailedTransition { get; set; }

    bool ThrowExceptionOnSameStateTransition { get; set; }

    /// <summary>
    /// 
    /// </summary>
    TState CurrentState { get; }

    /// <summary>
    /// 
    /// </summary>
    IEnumerable<HistoryItem<TState, TStimulus>> History { get; }

    /// <summary>
    /// Forcefully sets the state of the state machine.  No actions will trigger
    /// </summary>
    /// <param name="state"></param>
    void OverrideState(TState state);

    /// <summary>
    /// Injects a <see cref="TStimulus"/> into the machine
    /// </summary>
    /// <param name="stimulus"></param>
    /// <returns></returns>
    bool Post(TStimulus stimulus);
}