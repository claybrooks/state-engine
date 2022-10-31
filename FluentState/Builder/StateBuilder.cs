using System;
using System.Collections.Generic;
using FluentState.MachineParts;

namespace FluentState.Builder;

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
    IStateBuilder<TState, TStimulus> CanTransitionTo(TState to, TStimulus reason, IEnumerable<IAction<TState, TStimulus>>? actions = null, IEnumerable<IGuard<TState, TStimulus>>? guards = null);

    /// <summary>
    /// Allows a transition from <paramref name="from"/> to this state with optional <paramref name="actions"/> and <paramref name="guards"/>
    /// </summary>
    /// <param name="from"></param>
    /// <param name="reason"></param>
    /// <param name="guards"></param>
    /// <param name="actions"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> CanTransitionFrom(TState from, TStimulus reason, IEnumerable<IAction<TState, TStimulus>>? actions = null, IEnumerable<IGuard<TState, TStimulus>>? guards = null);

    /// <summary>
    /// Register guard whenever transition to <paramref name="to"/> because of <paramref name="reason"/>
    /// </summary>
    /// <param name="to"></param>
    /// <param name="reason"></param>
    /// <param name="guard"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveGuard(TState to, TStimulus reason, Func<Transition<TState, TStimulus>, bool> guard);
    /// <summary>
    /// Register guard whenever transition to <paramref name="to"/> because of <paramref name="reason"/>
    /// </summary>
    /// <typeparam name="TGuard"></typeparam>
    /// <param name="to"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveGuard<TGuard>(TState to, TStimulus reason) where TGuard : IGuard<TState, TStimulus>, new();
    /// <summary>
    /// Register guard whenever transition to <paramref name="to"/> because of <paramref name="reason"/>
    /// </summary>
    /// <param name="to"></param>
    /// <param name="reason"></param>
    /// <param name="guard"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveGuard(TState to, TStimulus reason, IGuard<TState, TStimulus> guard);

    /// <summary>
    /// Register guard whenever transitioning from <paramref name="from"/> because of <paramref name="reason"/>
    /// </summary>
    /// <param name="from"></param>
    /// <param name="reason"></param>
    /// <param name="guard"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterGuard(TState from, TStimulus reason, Func<Transition<TState, TStimulus>, bool> guard);
    /// <summary>
    /// Register guard whenever transitioning from <paramref name="from"/> because of <paramref name="reason"/>
    /// </summary>
    /// <typeparam name="TGuard"></typeparam>
    /// <param name="from"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterGuard<TGuard>(TState from, TStimulus reason) where TGuard : IGuard<TState, TStimulus>, new();
    /// <summary>
    /// Register guard whenever transitioning from <paramref name="from"/> because of <paramref name="reason"/>
    /// </summary>
    /// <param name="from"></param>
    /// <param name="reason"></param>
    /// <param name="guard"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterGuard(TState from, TStimulus reason, IGuard<TState, TStimulus> guard);

    /// <summary>
    /// Adds a trigger to fire whenever this state is entered
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterAction(Action<Transition<TState, TStimulus>> action);

    /// <summary>
    /// Adds a trigger to fire whenever this state is entered
    /// </summary>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterAction<TAction>() where TAction : IAction<TState, TStimulus>, new();

    /// <summary>
    /// Adds a trigger to fire whenever this state is entered
    /// </summary>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterAction(IAction<TState, TStimulus> action);

    /// <summary>
    /// Adds a trigger to fire whenever this state is entered from <paramref name="from"/> when <paramref name="reason"/>
    /// </summary>
    /// <param name="from"></param>
    /// <param name="reason"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterAction(TState from, TStimulus reason, Action<Transition<TState, TStimulus>> action);
    
    /// <summary>
    /// Adds a trigger to fire whenever this state is entered from <paramref name="from"/> when <paramref name="reason"/>
    /// </summary>
    /// <typeparam name="TAction"></typeparam>
    /// <param name="from"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterAction<TAction>(TState from, TStimulus reason) where TAction : IAction<TState, TStimulus>, new();

    /// <summary>
    /// Adds a trigger to fire whenever this state is entered from <paramref name="from"/> when <paramref name="reason"/>
    /// </summary>
    /// <param name="from"></param>
    /// <param name="reason"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithEnterAction(TState from, TStimulus reason, IAction<TState, TStimulus> action);

    /// <summary>
    /// Adds a trigger to fire whenever this state is left
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveAction(Action<Transition<TState, TStimulus>> action);
    
    /// <summary>
    /// Adds a trigger to fire whenever this state is left
    /// </summary>
    /// <typeparam name="TAction"></typeparam>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveAction<TAction>() where TAction : IAction<TState, TStimulus>, new();

    /// <summary>
    /// Adds a trigger to fire whenever this state is left
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveAction(IAction<TState, TStimulus> action);

    /// <summary>
    /// Adds a trigger to fire whenever this state is left to <paramref name="to"/> when <paramref name="reason"/>
    /// </summary>
    /// <param name="to"></param>
    /// <param name="reason"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveAction(TState to, TStimulus reason, Action<Transition<TState, TStimulus>> action);

    /// <summary>
    /// Adds a trigger to fire whenever this state is left to <paramref name="to"/> when <paramref name="reason"/>
    /// </summary>
    /// <typeparam name="TAction"></typeparam>
    /// <param name="to"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveAction<TAction>(TState to, TStimulus reason) where TAction : IAction<TState, TStimulus>, new();

    /// <summary>
    /// Adds a trigger to fire whenever this state is left to <paramref name="to"/> when <paramref name="reason"/>
    /// </summary>
    /// <param name="to"></param>
    /// <param name="reason"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    IStateBuilder<TState, TStimulus> WithLeaveAction(TState to, TStimulus reason, IAction<TState, TStimulus> action);
}

public sealed class StateBuilder<TState, TStimulus> : IStateBuilder<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    private readonly TState _state;

    private readonly IStateGuard<TState, TStimulus> _guard;
    private readonly IStateMap<TState, TStimulus> _stateMap;
    private readonly IActionRegistry<TState, TStimulus> _enterActions;
    private readonly IActionRegistry<TState, TStimulus> _leaveActions;

    public StateBuilder(TState state,
        IStateGuard<TState, TStimulus> guard,
        IStateMap<TState, TStimulus> stateMap,
        IActionRegistry<TState, TStimulus> enterActions,
        IActionRegistry<TState, TStimulus> leaveActions)
    {
        _state = state;
        _guard = guard;
        _stateMap = stateMap;
        _enterActions = enterActions;
        _leaveActions = leaveActions;
    }

    #region Transitions

    public IStateBuilder<TState, TStimulus> CanTransitionTo(TState to, TStimulus reason, IEnumerable<IAction<TState, TStimulus>>? actions = null, IEnumerable<IGuard<TState, TStimulus>>? guards = null)
    {
        var transition = new Transition<TState, TStimulus> {From = _state, To = to, Reason = reason };
        DoRegisterTransition(transition);
        return this;
    }

    public IStateBuilder<TState, TStimulus> CanTransitionFrom(TState from, TStimulus reason, IEnumerable<IAction<TState, TStimulus>>? actions = null, IEnumerable<IGuard<TState, TStimulus>>? guards = null)
    {
        var transition = new Transition<TState, TStimulus> {From = from, To = _state, Reason = reason };
        DoRegisterTransition(transition);
        return this;
    }

    private void DoRegisterTransition(Transition<TState, TStimulus> transition, IEnumerable<IAction<TState, TStimulus>>? actions = null, IEnumerable<IGuard<TState, TStimulus>>? guards = null)
    {
        guards ??= Array.Empty<IGuard<TState, TStimulus>>();
        actions ??= Array.Empty<IAction<TState, TStimulus>>();

        _stateMap.Register(transition);

        foreach (var action in actions)
        {
            _leaveActions.Register(action);
        }

        foreach (var guard in guards)
        {
            _guard.Register(transition, guard);
        }
    }

    #endregion

    #region Enter Guards

    public IStateBuilder<TState, TStimulus> WithEnterGuard(TState from, TStimulus reason, Func<Transition<TState, TStimulus>, bool> guard)
    {
        return WithEnterGuard(from, reason, new DelegateGuard<TState, TStimulus>(guard));
    }

    public IStateBuilder<TState, TStimulus> WithEnterGuard<TGuard>(TState from, TStimulus reason)
        where TGuard : IGuard<TState, TStimulus>, new()
    {
        return WithEnterGuard(from, reason, new TGuard());
    }

    public IStateBuilder<TState, TStimulus> WithEnterGuard(TState from, TStimulus reason, IGuard<TState, TStimulus> guard)
    {
        _guard.Register(new Transition<TState, TStimulus> {From = from, To = _state, Reason = reason}, guard);
        return this;
    }

    #endregion

    #region Leave Guards

    public IStateBuilder<TState, TStimulus> WithLeaveGuard(TState to, TStimulus reason, Func<Transition<TState, TStimulus>, bool> guard)
    {
        return WithLeaveGuard(to, reason, new DelegateGuard<TState, TStimulus>(guard));
    }

    public IStateBuilder<TState, TStimulus> WithLeaveGuard<TGuard>(TState to, TStimulus reason)
        where TGuard : IGuard<TState, TStimulus>, new()
    {
        return WithLeaveGuard(to, reason, new TGuard());
    }

    public IStateBuilder<TState, TStimulus> WithLeaveGuard(TState to, TStimulus reason, IGuard<TState, TStimulus> guard)
    {
        _guard.Register(new Transition<TState, TStimulus> {From = _state, To = to, Reason = reason}, guard);
        return this;
    }

    #endregion

    #region Global State Enter Transitions

    public IStateBuilder<TState, TStimulus> WithEnterAction(Action<Transition<TState, TStimulus>> action)
    {
        return WithEnterAction(new DelegateAction<TState, TStimulus>(action));
    }

    public IStateBuilder<TState, TStimulus> WithEnterAction<TAction>() where TAction : IAction<TState, TStimulus>, new()
    {
        return WithEnterAction(new TAction());
    }

    public IStateBuilder<TState, TStimulus> WithEnterAction(IAction<TState, TStimulus> action)
    {
        _enterActions.Register(action);
        return this;
    }

    #endregion

    #region State Enter Transitions

    public IStateBuilder<TState, TStimulus> WithEnterAction(TState from, TStimulus reason, Action<Transition<TState, TStimulus>> action)
    {
        return WithEnterAction(from, reason, new DelegateAction<TState, TStimulus>(action));
    }

    public IStateBuilder<TState, TStimulus> WithEnterAction<TAction>(TState from, TStimulus reason) where TAction : IAction<TState, TStimulus>, new()
    {
        return WithEnterAction(from, reason, new TAction());
    }

    public IStateBuilder<TState, TStimulus> WithEnterAction(TState from, TStimulus reason, IAction<TState, TStimulus> action)
    {
        _enterActions.Register(new Transition<TState, TStimulus>{From = from, To = _state, Reason = reason}, action);
        return this;
    }

    #endregion

    #region Global State Leave Transitions

    public IStateBuilder<TState, TStimulus> WithLeaveAction(Action<Transition<TState, TStimulus>> action)
    {
        return WithLeaveAction(new DelegateAction<TState, TStimulus>(action));
    }

    public IStateBuilder<TState, TStimulus> WithLeaveAction<TAction>() where TAction : IAction<TState, TStimulus>, new()
    {
        return WithLeaveAction(new TAction());
    }

    public IStateBuilder<TState, TStimulus> WithLeaveAction(IAction<TState, TStimulus> action)
    {
        _leaveActions.Register(action);
        return this;
    }

    #endregion

    #region State Leave Transitions
    public IStateBuilder<TState, TStimulus> WithLeaveAction(TState to, TStimulus reason, Action<Transition<TState, TStimulus>> action)
    {
        return WithLeaveAction(to, reason, new DelegateAction<TState, TStimulus>(action));
    }

    public IStateBuilder<TState, TStimulus> WithLeaveAction<TAction>(TState to, TStimulus reason) where TAction : IAction<TState, TStimulus>, new()
    {
        return WithLeaveAction(to, reason, new TAction());
    }

    public IStateBuilder<TState, TStimulus> WithLeaveAction(TState to, TStimulus reason, IAction<TState, TStimulus> action)
    {
        _leaveActions.Register(_state, to, reason, action);
        return this;
    }

    #endregion
}