using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace FluentState;

public interface IStateMachine<out TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    /// <summary>
    /// This does not consider failed transitions due to transitionGuardRegistry calls.  Only when there is no destination state based on
    /// <see cref="IStateMachine{TState,TStimulus}.CurrentState"/> and the provided <typeparamref name="TStimulus"/>
    /// </summary>
    bool ThrowExceptionOnFailedTransition { get; set; }

    /// <summary>
    /// Should most likely be turned off outside of development.  Helpful in determining possible bugs in your state machine
    /// setup
    /// </summary>
    bool ThrowExceptionOnSameStateTransition { get; set; }
    
    TState CurrentState { get; }

    /// <summary>
    /// Queue's the provided <typeparamref name="TStimulus"/> to the state machine, but defers execution.
    /// </summary>
    /// <param name="stimulus"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> Post(TStimulus stimulus, CancellationToken token = default);
    
    IEnumerable<IHistoryItem<TState, TStimulus>> History { get; }
}