using System;

namespace FluentState.Builder
{
    public interface IStateMachineBuilder<TStateMachine, TState, TStimulus>
        where TStateMachine : IStateMachine<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        /// <summary>
        /// Retrieve the current <typeparamref name="TStateMachine"/> being build.  Callers should avoid this unless there is a compelling reason.
        /// Implemenations of <see cref="FluentState.Persistence.ISerializer{TState, TStimulus}"/> typically need access to the lower level machine
        /// for various reasons.
        /// </summary>
        IStateMachine<TState, TStimulus> Machine { get; }

        /// <summary>
        /// Begin configuring a speicifc <typeparamref name="TState"/>
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        IStateBuilder<TStateMachine, TState, TStimulus> WithState(TState state);

        /// <summary>
        /// Register an action to be called whenenver any state is entered
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IStateMachineBuilder<TStateMachine, TState, TStimulus> WithEnterAction(Action<TState, TState, TStimulus> action);

        /// <summary>
        /// Register an action to be called whenenver any state is left
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IStateMachineBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(Action<TState, TState, TStimulus> action);

        /// <summary>
        /// Configure the <typeparamref name="TStateMachine"/> history as unbounded.  This will implicitly enable history.
        /// </summary>
        /// <remarks>
        /// Callers can disable history at any time.
        /// </remarks>
        /// <returns></returns>
        IStateMachineBuilder<TStateMachine, TState, TStimulus> WithUnboundedHistory();

        /// <summary>
        /// Configure the <typeparamref name="TStateMachine"/> history as bounded to a specific size.  This will implicitly
        /// enable history.
        /// </summary>
        /// <remarks>
        /// Callers can disable history at any time.
        /// </remarks>
        /// <param name="size"></param>
        /// <returns></returns>
        IStateMachineBuilder<TStateMachine, TState, TStimulus> WithBoundedHistory(int size);

        /// <summary>
        /// End configuring <typeparamref name="TStateMachine"/>
        /// </summary>
        /// <returns></returns>
        TStateMachine Build();
    }
}
