using FluentState.Machine;
using System;
using System.Collections.Generic;

namespace FluentState.Builder
{
    public interface IStateBuilder<TStateMachine, TState, TStimulus>
        where TStateMachine : IStateMachine<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        /// <summary>
        /// Register a state transition pair.
        /// </summary>
        /// <param name="enteringState"></param>
        /// <param name="reason"></param>
        /// <param name="guards"></param>
        /// <param name="enterActions"></param>
        /// <param name="leaveActions"></param>
        /// <returns></returns>
        IStateBuilder<TStateMachine, TState, TStimulus> WithTransitionTo(
            TState enteringState,
            TStimulus reason,
            IEnumerable<Func<TState, TState, TStimulus, bool>>? guards = null,
            IEnumerable<Action<TState, TState, TStimulus>>? enterActions = null,
            IEnumerable<Action<TState, TState, TStimulus>>? leaveActions = null
        );

        /// <summary>
        /// <see cref="IStateMachine{TState, TStimulus}.AddStateEnterAction(TState, Action{TState, TState, TStimulus})"/>
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IStateBuilder<TStateMachine, TState, TStimulus> WithEnterAction(Action<TState, TState, TStimulus> action);
        
        /// <summary>
        /// <see cref="IStateMachine{TState, TStimulus}.AddStateLeaveAction(TState, Action{TState, TState, TStimulus})"/>
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IStateBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(Action<TState, TState, TStimulus> action);

        /// <summary>
        /// <see cref="IStateMachine{TState, TStimulus}.AddStateEnterAction(TState, TState, TStimulus, Action{TState, TState, TStimulus})"/>
        /// </summary>
        /// <param name="enteringState"></param>
        /// <param name="reason"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        IStateBuilder<TStateMachine, TState, TStimulus> WithEnterAction(TState enteringState, TStimulus reason, Action<TState, TState, TStimulus> action);

        /// <summary>
        /// <see cref="IStateMachine{TState, TStimulus}.AddStateLeaveAction(TState, TState, TStimulus, Action{TState, TState, TStimulus})"/>
        /// </summary>
        /// <param name="leavingState"></param>
        /// <param name="reason"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        IStateBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(TState leavingState, TStimulus reason, Action<TState, TState, TStimulus> action);

        /// <summary>
        /// Concludes the configuration of this state.
        /// </summary>
        /// <returns></returns>
        IStateMachineBuilder<TStateMachine, TState, TStimulus> Build();
    }
}
