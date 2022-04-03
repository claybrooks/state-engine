using FluentState.History;
using System;

namespace FluentState.Machine
{
    public interface IStateMachine<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        
        /// <summary>
        /// Retrieve the current state of the state machine.
        /// </summary>
        TState CurrentState { get; }

        /// <summary>
        /// Retrieve the history of the state machine.
        /// </summary>
        IStateMachineHistory<TState, TStimulus> History { get; }


        /// <summary>
        /// Users should typically avoid calling this, unless there is a specific reason why the state needs to be overridden.
        /// Internally, most implementations of the ISerializer interface will call this to set the state machines current state.
        /// Outside of serialization, this method should be avoided
        /// </summary>
        /// <param name="state"></param>
        void OverrideState(TState state);

        /// <summary>
        /// Transition guards are used to allow or block transitions between states.  These guards will be invoked whenever there
        /// is a transition between the provided <typeparamref name="TState"/>'s due to the provided <typeparamref name="TStimulus"/>.
        /// If there are multiple stimuli that can cause a transition between pairs of states, this function should be called multiple times.
        /// </summary>
        /// <param name="enteringState"></param>
        /// <param name="leavingState"></param>
        /// <param name="when"></param>
        /// <param name="guard"></param>
        /// <returns></returns>
        bool AddTransitionGuard(TState enteringState, TState leavingState, TStimulus when, Func<TState, TState, TStimulus, bool> guard);

        /// <summary>
        /// Registers an allowable transition between the provided <typeparamref name="TState"/>'s
        /// </summary>
        /// <param name="enteringState"></param>
        /// <param name="leavingState"></param>
        /// <param name="when"></param>
        /// <returns></returns>
        bool AddTransition(TState enteringState, TState leavingState, TStimulus when);

        /// <summary>
        /// Registers an action to be run whenever any state is entered.
        /// </summary>
        /// <param name="action"></param>
        void AddStateEnterAction(Action<TState, TState, TStimulus> action);
        
        /// <summary>
        /// Registers an action to be run whenever a specific state is entered regardless of the previous state or why the state
        /// transition happened.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void AddStateEnterAction(TState state, Action<TState, TState, TStimulus> action);

        /// <summary>
        /// Registers an action to be run when entering the provided <paramref name="enteringState"/> but only when leaving the provided
        /// <paramref name="leavingState"/> and the transition is due to the provided <paramref name="reason"/>.
        /// If there are multiple stimuli that can cause a specific state transition pair to occur, the action must be registered for each
        /// distinct <typeparamref name="TStimulus"/> that should trigger the action.
        /// </summary>
        /// <param name="enteringState"></param>
        /// <param name="leavingState"></param>
        /// <param name="reason"></param>
        /// <param name="action"></param>
        void AddStateEnterAction(TState enteringState, TState leavingState, TStimulus reason, Action<TState, TState, TStimulus> action);

        /// <summary>
        /// Registers an action to be run whenever any state is left.
        /// </summary>
        /// <param name="action"></param>
        void AddStateLeaveAction(Action<TState, TState, TStimulus> action);

        /// <summary>
        /// Registers an action to be run whenever a specific state is left regardless of the current state or why the state
        /// transition happened.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void AddStateLeaveAction(TState state, Action<TState, TState, TStimulus> action);

        /// <summary>
        /// Registers an action to be run when leaving the provided <paramref name="leavingState"/> but only when entering the provided
        /// <paramref name="enteringState"/> and the transition is due to the provided <paramref name="reason"/>.
        /// If there are multiple stimuli that can cause a specific state transition pair to occur, the action must be registered for each
        /// distinct <typeparamref name="TStimulus"/> that should trigger the action.
        /// </summary>
        /// <param name="enteringState"></param>
        /// <param name="leavingState"></param>
        /// <param name="reason"></param>
        /// <param name="action"></param>
        void AddStateLeaveAction(TState enteringState, TState leavingState, TStimulus reason, Action<TState, TState, TStimulus> action);

        /// <summary>
        /// Injects the provided <typeparamref name="TStimulus"/> to the state machine.  The mechanics of this API call is dependant upon
        /// the implementation of the state machine.  Check the documentation of the implemenation for further details.
        /// </summary>
        /// <param name="stimulus"></param>
        /// <returns></returns>
        bool Post(TStimulus stimulus);
    }
}
