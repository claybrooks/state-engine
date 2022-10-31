using FluentState.History;
using FluentState.MachineParts;
using System;

namespace FluentState.Builder;

public interface IBuilder<TStateMachine, TState, TStimulus>
    where TStateMachine : IStateMachine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    /// <summary>
    /// Enables a state within the <see cref="IStateMachine{TState,TStimulus}"/>
    /// </summary>
    /// <param name="state"></param>
    /// <param name="configureState"></param>
    /// <returns></returns>
    IBuilder<TStateMachine, TState, TStimulus> WithState(TState state, Action<StateBuilder<TState, TStimulus>> configureState);

    /// <summary>
    /// Adds a trigger for any state enter event
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    IBuilder<TStateMachine, TState, TStimulus> WithEnterAction(Action<Transition<TState, TStimulus>> action);

    /// <summary>
    /// Adds a trigger for any state enter event
    /// </summary>
    /// <typeparam name="TAction"></typeparam>
    /// <returns></returns>
    IBuilder<TStateMachine, TState, TStimulus> WithEnterAction<TAction>() where TAction : IAction<TState, TStimulus>, new();

    /// <summary>
    /// Adds a trigger for any state enter event
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    IBuilder<TStateMachine, TState, TStimulus> WithEnterAction(IAction<TState, TStimulus> action);

    /// <summary>
    /// Adds a trigger for any state leave event
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    IBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(Action<Transition<TState, TStimulus>> action);

    /// <summary>
    /// Adds a trigger for any state leave event
    /// </summary>
    /// <typeparam name="TAction"></typeparam>
    /// <returns></returns>
    IBuilder<TStateMachine, TState, TStimulus> WithLeaveAction<TAction>() where TAction : IAction<TState, TStimulus>, new();

    /// <summary>
    /// Adds a trigger for any state leave event
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    IBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(IAction<TState, TStimulus> action);

    /// <summary>
    /// Enables <see cref="IStateMachine{TState,TStimulus}"/> history and makes it unbounded
    /// </summary>
    /// <returns></returns>
    IBuilder<TStateMachine, TState, TStimulus> WithUnboundedHistory();

    /// <summary>
    /// Enables <see cref="IStateMachine{TState,TStimulus}"/> history and bounds it to <paramref name="size"/>
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    IBuilder<TStateMachine, TState, TStimulus> WithBoundedHistory(int size);

    /// <summary>
    /// Builds the <see cref="IStateMachine{TState,TStimulus}"/>
    /// </summary>
    /// <returns></returns>
    TStateMachine Build();
}

public class Builder<TStateMachine, TState, TStimulus> : IBuilder<TStateMachine, TState, TStimulus>
    where TStateMachine : IStateMachine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    private readonly TState _initialState;
    private readonly IStateMachineFactory<TStateMachine, TState, TStimulus> _factory;
    
    private readonly StateGuard<TState, TStimulus> _guard = new();
    private readonly StateMap<TState, TStimulus> _stateMap = new();
    private readonly ActionRegistry<TState, TStimulus> _enterActions = new();
    private readonly ActionRegistry<TState, TStimulus> _leaveActions = new();
    private readonly StateMachineHistory<TState, TStimulus> _history = new();

    public Builder(TState initialState, IStateMachineFactory<TStateMachine, TState, TStimulus> factory)
    {
        _initialState = initialState;
        _factory = factory;
    }

    public IBuilder<TStateMachine, TState, TStimulus> WithState(TState state, Action<StateBuilder<TState, TStimulus>> configureState)
    {
        var state_builder = new StateBuilder<TState, TStimulus>(state, _guard, _stateMap, _enterActions, _leaveActions);
        configureState(state_builder);
        return this;
    }

    #region Global Enter Actions

    public IBuilder<TStateMachine, TState, TStimulus> WithEnterAction(Action<Transition<TState, TStimulus>> action)
    {
        return WithEnterAction(new DelegateAction<TState, TStimulus>(action));
    }

    public IBuilder<TStateMachine, TState, TStimulus> WithEnterAction<TAction>() where TAction : IAction<TState, TStimulus>, new()
    {
        return WithEnterAction(new TAction());
    }
    public IBuilder<TStateMachine, TState, TStimulus> WithEnterAction(IAction<TState, TStimulus> action)
    {
        _enterActions.Register(action);
        return this;
    }

    #endregion

    #region Global Leave Actions

    public IBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(Action<Transition<TState, TStimulus>> action)
    {
        return WithLeaveAction(new DelegateAction<TState, TStimulus>(action));
    }

    public IBuilder<TStateMachine, TState, TStimulus> WithLeaveAction<TAction>() where TAction : IAction<TState, TStimulus>, new()
    {
        return WithLeaveAction(new TAction());
    }

    public IBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(IAction<TState, TStimulus> action)
    {
        _leaveActions.Register(action);
        return this;
    }

    #endregion

    public IBuilder<TStateMachine, TState, TStimulus> WithUnboundedHistory()
    {
        _history.Enabled = true;
        _history.MakeUnbounded();
        return this;
    }

    public IBuilder<TStateMachine, TState, TStimulus> WithBoundedHistory(int size)
    {
        _history.Enabled = true;
        _history.MakeBounded(size);
        return this;
    }

    public TStateMachine  Build()
    {
        return _factory.Create(_initialState, _enterActions, _leaveActions, _stateMap, _guard, _history);
    }
}