using FluentState.History;
using System.Collections.Generic;
using FluentState.Machine;

namespace FluentState;

public interface IStateMachine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    /// <summary>
    /// This does not consider failed transitions due to guard calls.  Only when there is no destination state based on
    /// <see cref="SynchronousStateMachine{TState,TStimulus}.CurrentState"/> and the provided <typeparamref name="TStimulus"/>
    /// </summary>
    bool ThrowExceptionOnFailedTransition { get; set; }

    /// <summary>
    /// Should most likely be turned off outside of development.  Helpful in determining possible bugs in your state machine
    /// setup
    /// </summary>
    bool ThrowExceptionOnSameStateTransition { get; set; }
    
    TState CurrentState { get; }
    
    IEnumerable<HistoryItem<TState, TStimulus>> History { get; }
}