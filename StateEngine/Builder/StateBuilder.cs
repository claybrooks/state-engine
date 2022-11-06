using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace StateEngine;

public interface IStateBuilder<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    /// <summary>
    /// Allows a transition from this state to <paramref name="to"/> with optional <paramref name="actions"/> and <paramref name="guards"/>
    /// </summary>
    /// <param name="to"></param>
    /// <param name="reason"></param>
    /// <param name="guards"></param>
    /// <param name="actions"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> CanTransitionTo(TState to, TStimulus reason, IEnumerable<ITransitionAction<TState, TStimulus>>? actions = null, IEnumerable<ITransitionGuard<TState, TStimulus>>? guards = null);

    /// <summary>
    /// Allows a transition from <paramref name="from"/> to this state with optional <paramref name="actions"/> and <paramref name="guards"/>
    /// </summary>
    /// <param name="from"></param>
    /// <param name="reason"></param>
    /// <param name="guards"></param>
    /// <param name="actions"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> CanTransitionFrom(TState from, TStimulus reason, IEnumerable<ITransitionAction<TState, TStimulus>>? actions = null, IEnumerable<ITransitionGuard<TState, TStimulus>>? guards = null);

    /// <summary>
    /// Register guardRegistry whenever transition to <paramref name="to"/> because of <paramref name="reason"/>
    /// </summary>
    /// <param name="to"></param>
    /// <param name="reason"></param>
    /// <param name="guard"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveGuard(TState to, TStimulus reason, Func<ITransition<TState, TStimulus>, bool> guard);
    /// <summary>
    /// Register guardRegistry whenever transition to <paramref name="to"/> because of <paramref name="reason"/>
    /// </summary>
    /// <typeparam name="TGuard"></typeparam>
    /// <param name="to"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveGuard<TGuard>(TState to, TStimulus reason) where TGuard : ITransitionGuard<TState, TStimulus>, new();
    /// <summary>
    /// Register guardRegistry whenever transition to <paramref name="to"/> because of <paramref name="reason"/>
    /// </summary>
    /// <param name="to"></param>
    /// <param name="reason"></param>
    /// <param name="transitionGuard"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveGuard(TState to, TStimulus reason, ITransitionGuard<TState, TStimulus> transitionGuard);

    /// <summary>
    /// Register guardRegistry whenever transitioning from <paramref name="from"/> because of <paramref name="reason"/>
    /// </summary>
    /// <param name="from"></param>
    /// <param name="reason"></param>
    /// <param name="guard"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterGuard(TState from, TStimulus reason, Func<ITransition<TState, TStimulus>, bool> guard);
    /// <summary>
    /// Register guardRegistry whenever transitioning from <paramref name="from"/> because of <paramref name="reason"/>
    /// </summary>
    /// <typeparam name="TGuard"></typeparam>
    /// <param name="from"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterGuard<TGuard>(TState from, TStimulus reason) where TGuard : ITransitionGuard<TState, TStimulus>, new();
    /// <summary>
    /// Register guardRegistry whenever transitioning from <paramref name="from"/> because of <paramref name="reason"/>
    /// </summary>
    /// <param name="from"></param>
    /// <param name="reason"></param>
    /// <param name="transitionGuard"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterGuard(TState from, TStimulus reason, ITransitionGuard<TState, TStimulus> transitionGuard);

    /// <summary>
    /// Adds a trigger to fire whenever this state is entered
    /// </summary>
    /// <param name="action"></param>
    /// <param name="idOverride"></param>
    /// <param name="filePath"></param>
    /// <param name="caller"></param>
    /// <param name="lineNumber"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterAction(Action<ITransition<TState, TStimulus>> action, string? idOverride = null, [CallerFilePath] string filePath = "", [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Adds a trigger to fire whenever this state is entered
    /// </summary>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterAction<TAction>() where TAction : ITransitionAction<TState, TStimulus>, new();

    /// <summary>
    /// Adds a trigger to fire whenever this state is entered
    /// </summary>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterAction(ITransitionAction<TState, TStimulus> transitionAction);

    /// <summary>
    /// Adds a trigger to fire whenever this state is entered from <paramref name="from"/> when <paramref name="reason"/>
    /// </summary>
    /// <param name="from"></param>
    /// <param name="reason"></param>
    /// <param name="action"></param>
    /// <param name="idOverride"></param>
    /// <param name="filePath"></param>
    /// <param name="caller"></param>
    /// <param name="lineNumber"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterAction(TState from, TStimulus reason, Action<ITransition<TState, TStimulus>> action, string? idOverride = null, [CallerFilePath] string filePath = "", [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0);
    
    /// <summary>
    /// Adds a trigger to fire whenever this state is entered from <paramref name="from"/> when <paramref name="reason"/>
    /// </summary>
    /// <typeparam name="TAction"></typeparam>
    /// <param name="from"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterAction<TAction>(TState from, TStimulus reason) where TAction : ITransitionAction<TState, TStimulus>, new();

    /// <summary>
    /// Adds a trigger to fire whenever this state is entered from <paramref name="from"/> when <paramref name="reason"/>
    /// </summary>
    /// <param name="from"></param>
    /// <param name="reason"></param>
    /// <param name="transitionAction"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterAction(TState from, TStimulus reason, ITransitionAction<TState, TStimulus> transitionAction);

    /// <summary>
    /// Adds a trigger to fire whenever this state is left
    /// </summary>
    /// <param name="action"></param>
    /// <param name="idOverride"></param>
    /// <param name="filePath"></param>
    /// <param name="caller"></param>
    /// <param name="lineNumber"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveAction(Action<ITransition<TState, TStimulus>> action, string? idOverride = null, [CallerFilePath] string filePath = "", [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0);
    
    /// <summary>
    /// Adds a trigger to fire whenever this state is left
    /// </summary>
    /// <typeparam name="TAction"></typeparam>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveAction<TAction>() where TAction : ITransitionAction<TState, TStimulus>, new();

    /// <summary>
    /// Adds a trigger to fire whenever this state is left
    /// </summary>
    /// <param name="transitionAction"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveAction(ITransitionAction<TState, TStimulus> transitionAction);

    /// <summary>
    /// Adds a trigger to fire whenever this state is left to <paramref name="to"/> when <paramref name="reason"/>
    /// </summary>
    /// <param name="to"></param>
    /// <param name="reason"></param>
    /// <param name="action"></param>
    /// <param name="idOverride"></param>
    /// <param name="filePath"></param>
    /// <param name="caller"></param>
    /// <param name="lineNumber"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveAction(TState to, TStimulus reason, Action<ITransition<TState, TStimulus>> action, string? idOverride = null, [CallerFilePath] string filePath = "", [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Adds a trigger to fire whenever this state is left to <paramref name="to"/> when <paramref name="reason"/>
    /// </summary>
    /// <typeparam name="TAction"></typeparam>
    /// <param name="to"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveAction<TAction>(TState to, TStimulus reason) where TAction : ITransitionAction<TState, TStimulus>, new();

    /// <summary>
    /// Adds a trigger to fire whenever this state is left to <paramref name="to"/> when <paramref name="reason"/>
    /// </summary>
    /// <param name="to"></param>
    /// <param name="reason"></param>
    /// <param name="transitionAction"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveAction(TState to, TStimulus reason, ITransitionAction<TState, TStimulus> transitionAction);
}

internal sealed class StateBuilder<TState, TStimulus> : IStateBuilder<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    private readonly TState _state;

    private readonly IGuardRegistry<TState, TStimulus> _guardRegistry;
    private readonly IStateMap<TState, TStimulus> _stateMap;
    private readonly IActionRegistry<TState, TStimulus> _enterActionRegistry;
    private readonly IActionRegistry<TState, TStimulus> _leaveActionRegistry;

    public StateBuilder(TState state,
        IGuardRegistry<TState, TStimulus> guardRegistry,
        IStateMap<TState, TStimulus> stateMap,
        IActionRegistry<TState, TStimulus> enterActionRegistry,
        IActionRegistry<TState, TStimulus> leaveActionRegistry)
    {
        _state = state;
        _guardRegistry = guardRegistry;
        _stateMap = stateMap;
        _enterActionRegistry = enterActionRegistry;
        _leaveActionRegistry = leaveActionRegistry;
    }

    #region Transitions

    public IStateBuilder<TState, TStimulus> CanTransitionTo(TState to, TStimulus reason, IEnumerable<ITransitionAction<TState, TStimulus>>? actions = null, IEnumerable<ITransitionGuard<TState, TStimulus>>? guards = null)
    {
        var transition = new Transition<TState, TStimulus> {From = _state, To = to, Reason = reason };
        DoRegisterTransition(transition);
        return this;
    }

    public IStateBuilder<TState, TStimulus> CanTransitionFrom(TState from, TStimulus reason, IEnumerable<ITransitionAction<TState, TStimulus>>? actions = null, IEnumerable<ITransitionGuard<TState, TStimulus>>? guards = null)
    {
        var transition = new Transition<TState, TStimulus> {From = from, To = _state, Reason = reason };
        DoRegisterTransition(transition);
        return this;
    }

    private void DoRegisterTransition(ITransition<TState, TStimulus> transition, IEnumerable<ITransitionAction<TState, TStimulus>>? actions = null, IEnumerable<ITransitionGuard<TState, TStimulus>>? guards = null)
    {
        guards ??= Array.Empty<ITransitionGuard<TState, TStimulus>>();
        actions ??= Array.Empty<ITransitionAction<TState, TStimulus>>();

        _stateMap.Register(transition);

        foreach (var action in actions)
        {
            _leaveActionRegistry.Register(action);
        }

        foreach (var guard in guards)
        {
            _guardRegistry.Register(transition, guard);
        }
    }

    #endregion

    #region Enter Guards

    public IStateBuilder<TState, TStimulus> WithEnterGuard(TState from, TStimulus reason, Func<ITransition<TState, TStimulus>, bool> guard)
    {
        return WithEnterGuard(from, reason, new DelegateTransitionGuard<TState, TStimulus>(guard));
    }

    public IStateBuilder<TState, TStimulus> WithEnterGuard<TGuard>(TState from, TStimulus reason)
        where TGuard : ITransitionGuard<TState, TStimulus>, new()
    {
        return WithEnterGuard(from, reason, new TGuard());
    }

    public IStateBuilder<TState, TStimulus> WithEnterGuard(TState from, TStimulus reason, ITransitionGuard<TState, TStimulus> transitionGuard)
    {
        _guardRegistry.Register(new Transition<TState, TStimulus> {From = from, To = _state, Reason = reason}, transitionGuard);
        return this;
    }

    #endregion

    #region Leave Guards

    public IStateBuilder<TState, TStimulus> WithLeaveGuard(TState to, TStimulus reason, Func<ITransition<TState, TStimulus>, bool> guard)
    {
        return WithLeaveGuard(to, reason, new DelegateTransitionGuard<TState, TStimulus>(guard));
    }

    public IStateBuilder<TState, TStimulus> WithLeaveGuard<TGuard>(TState to, TStimulus reason)
        where TGuard : ITransitionGuard<TState, TStimulus>, new()
    {
        return WithLeaveGuard(to, reason, new TGuard());
    }

    public IStateBuilder<TState, TStimulus> WithLeaveGuard(TState to, TStimulus reason, ITransitionGuard<TState, TStimulus> transitionGuard)
    {
        _guardRegistry.Register(new Transition<TState, TStimulus> {From = _state, To = to, Reason = reason}, transitionGuard);
        return this;
    }

    #endregion

    #region Global State Enter Transitions

    public IStateBuilder<TState, TStimulus> WithEnterAction(Action<ITransition<TState, TStimulus>> action, string? idOverride = null, [CallerFilePath] string filePath = "", [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0)
    {
        return WithEnterAction(new DelegateTransitionAction<TState, TStimulus>(action, idOverride, filePath, caller, lineNumber));
    }

    public IStateBuilder<TState, TStimulus> WithEnterAction<TAction>() where TAction : ITransitionAction<TState, TStimulus>, new()
    {
        return WithEnterAction(new TAction());
    }

    public IStateBuilder<TState, TStimulus> WithEnterAction(ITransitionAction<TState, TStimulus> transitionAction)
    {
        _enterActionRegistry.Register(transitionAction);
        return this;
    }

    #endregion

    #region State Enter Transitions

    public IStateBuilder<TState, TStimulus> WithEnterAction(TState from, TStimulus reason, Action<ITransition<TState, TStimulus>> action, string? idOverride = null, [CallerFilePath] string filePath = "", [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0)
    {
        return WithEnterAction(from, reason, new DelegateTransitionAction<TState, TStimulus>(action, idOverride, filePath, caller, lineNumber));
    }

    public IStateBuilder<TState, TStimulus> WithEnterAction<TAction>(TState from, TStimulus reason) where TAction : ITransitionAction<TState, TStimulus>, new()
    {
        return WithEnterAction(from, reason, new TAction());
    }

    public IStateBuilder<TState, TStimulus> WithEnterAction(TState from, TStimulus reason, ITransitionAction<TState, TStimulus> transitionAction)
    {
        _enterActionRegistry.Register(from, _state, reason, transitionAction);
        return this;
    }

    #endregion

    #region Global State Leave Transitions

    public IStateBuilder<TState, TStimulus> WithLeaveAction(Action<ITransition<TState, TStimulus>> action, string? idOverride = null, [CallerFilePath] string filePath = "", [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0)
    {
        return WithLeaveAction(new DelegateTransitionAction<TState, TStimulus>(action, idOverride, filePath, caller, lineNumber));
    }

    public IStateBuilder<TState, TStimulus> WithLeaveAction<TAction>() where TAction : ITransitionAction<TState, TStimulus>, new()
    {
        return WithLeaveAction(new TAction());
    }

    public IStateBuilder<TState, TStimulus> WithLeaveAction(ITransitionAction<TState, TStimulus> transitionAction)
    {
        _leaveActionRegistry.Register(transitionAction);
        return this;
    }

    #endregion

    #region State Leave Transitions
    public IStateBuilder<TState, TStimulus> WithLeaveAction(TState to, TStimulus reason, Action<ITransition<TState, TStimulus>> action, string? idOverride = null, [CallerFilePath] string filePath = "", [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0)
    {
        return WithLeaveAction(to, reason, new DelegateTransitionAction<TState, TStimulus>(action, idOverride, filePath, caller, lineNumber));
    }

    public IStateBuilder<TState, TStimulus> WithLeaveAction<TAction>(TState to, TStimulus reason) where TAction : ITransitionAction<TState, TStimulus>, new()
    {
        return WithLeaveAction(to, reason, new TAction());
    }

    public IStateBuilder<TState, TStimulus> WithLeaveAction(TState to, TStimulus reason, ITransitionAction<TState, TStimulus> transitionAction)
    {
        _leaveActionRegistry.Register(_state, to, reason, transitionAction);
        return this;
    }

    #endregion
}