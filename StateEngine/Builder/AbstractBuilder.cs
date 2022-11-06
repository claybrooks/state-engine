using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace StateEngine;

public interface IBuilder<out TStateMachine, TState, TStimulus>
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
    IBuilder<TStateMachine, TState, TStimulus> WithState(TState state, Action<IStateBuilder<TState, TStimulus>> configureState);

    /// <summary>
    /// Adds a trigger for any state enter event
    /// </summary>
    /// <param name="action"></param>
    /// <param name="idOverride"></param>
    /// <param name="filePath"></param>
    /// <param name="caller"></param>
    /// <param name="lineNumber"></param>
    /// <returns></returns>
    IBuilder<TStateMachine, TState, TStimulus> WithEnterAction(Action<ITransition<TState, TStimulus>> action, string? idOverride, [CallerFilePath] string filePath = "", [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Adds a trigger for any state enter event
    /// </summary>
    /// <typeparam name="TAction"></typeparam>
    /// <returns></returns>
    IBuilder<TStateMachine, TState, TStimulus> WithEnterAction<TAction>() where TAction : ITransitionAction<TState, TStimulus>, new();

    /// <summary>
    /// Adds a trigger for any state enter event
    /// </summary>
    /// <param name="transitionAction"></param>
    /// <returns></returns>
    IBuilder<TStateMachine, TState, TStimulus> WithEnterAction(ITransitionAction<TState, TStimulus> transitionAction);

    /// <summary>
    /// Adds a trigger for any state leave event
    /// </summary>
    /// <param name="action"></param>
    /// <param name="idOverride"></param>
    /// <param name="filePath"></param>
    /// <param name="caller"></param>
    /// <param name="lineNumber"></param>
    /// <returns></returns>
    IBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(Action<ITransition<TState, TStimulus>> action, string? idOverride, [CallerFilePath] string filePath = "", [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Adds a trigger for any state leave event
    /// </summary>
    /// <typeparam name="TAction"></typeparam>
    /// <returns></returns>
    IBuilder<TStateMachine, TState, TStimulus> WithLeaveAction<TAction>() where TAction : ITransitionAction<TState, TStimulus>, new();

    /// <summary>
    /// Adds a trigger for any state leave event
    /// </summary>
    /// <param name="transitionAction"></param>
    /// <returns></returns>
    IBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(ITransitionAction<TState, TStimulus> transitionAction);

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

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IValidator<TState, TStimulus> Validator<TValidatorFactory>(IEnumerable<IValidationRule<TState, TStimulus>> rules) where TValidatorFactory : IValidatorFactory<TState, TStimulus>, new();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TVisualizerFactory"></typeparam>
    /// <param name="configureRules"></param>
    /// <returns></returns>
    IVisualizer Visualizer<TVisualizerFactory>(Action<VisualizationRules<TState, TStimulus>> configureRules)
        where TVisualizerFactory : IVisualizerFactory<TState, TStimulus>, new();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TVisualizerFactory"></typeparam>
    /// <param name="rules"></param>
    /// <returns></returns>
    IVisualizer Visualizer<TVisualizerFactory>(VisualizationRules<TState, TStimulus>? rules = null)
        where TVisualizerFactory : IVisualizerFactory<TState, TStimulus>, new();
}

public abstract class AbstractBuilder<TStateMachine, TState, TStimulus> : IBuilder<TStateMachine, TState, TStimulus>
    where TStateMachine : IStateMachine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    private readonly TState _initialState;
    private readonly IStateMachineFactory<TStateMachine, TState, TStimulus> _factory;
    
    private readonly StateMap<TState, TStimulus> _stateMap = new();
    private readonly ActionRegistry<TState, TStimulus> _enterActionRegistry = new();
    private readonly ActionRegistry<TState, TStimulus> _leaveActionRegistry = new();
    private readonly GuardRegistry<TState, TStimulus> _guardRegistry = new();
    private readonly StateMachineHistory<TState, TStimulus> _history = new();

    protected AbstractBuilder(TState initialState, IStateMachineFactory<TStateMachine, TState, TStimulus> factory)
    {
        _initialState = initialState;
        _factory = factory;
    }

    public IBuilder<TStateMachine, TState, TStimulus> WithState(TState state,
        Action<IStateBuilder<TState, TStimulus>> configureState)
    {
        var state_builder =
            new StateBuilder<TState, TStimulus>(state, _guardRegistry, _stateMap, _enterActionRegistry, _leaveActionRegistry);
        configureState(state_builder);
        return this;
    }

    #region Global Enter Actions

    public IBuilder<TStateMachine, TState, TStimulus> WithEnterAction(Action<ITransition<TState, TStimulus>> action, string? idOverride, [CallerFilePath] string filePath = "", [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0)
    {
        return WithEnterAction(new DelegateTransitionAction<TState, TStimulus>(action, idOverride, filePath, caller, lineNumber));
    }

    public IBuilder<TStateMachine, TState, TStimulus> WithEnterAction<TAction>()
        where TAction : ITransitionAction<TState, TStimulus>, new()
    {
        return WithEnterAction(new TAction());
    }

    public IBuilder<TStateMachine, TState, TStimulus> WithEnterAction(ITransitionAction<TState, TStimulus> transitionAction)
    {
        _enterActionRegistry.Register(transitionAction);
        return this;
    }

    #endregion

    #region Global Leave Actions

    public IBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(Action<ITransition<TState, TStimulus>> action, string? idOverride, [CallerFilePath] string filePath = "", [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0)
    {
        return WithLeaveAction(new DelegateTransitionAction<TState, TStimulus>(action, idOverride, filePath, caller, lineNumber));
    }

    public IBuilder<TStateMachine, TState, TStimulus> WithLeaveAction<TAction>()
        where TAction : ITransitionAction<TState, TStimulus>, new()
    {
        return WithLeaveAction(new TAction());
    }

    public IBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(ITransitionAction<TState, TStimulus> transitionAction)
    {
        _leaveActionRegistry.Register(transitionAction);
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

    public IValidator<TState, TStimulus> Validator<TValidatorFactory>(IEnumerable<IValidationRule<TState, TStimulus>> rules) where TValidatorFactory : IValidatorFactory<TState, TStimulus>, new()
    {
        var factory = new TValidatorFactory();
        return factory.Create(rules, _initialState, _stateMap, _enterActionRegistry, _leaveActionRegistry,
            _guardRegistry);
    }

    public IVisualizer Visualizer<TVisualizerFactory>(Action<VisualizationRules<TState, TStimulus>> configureRules) where TVisualizerFactory : IVisualizerFactory<TState, TStimulus>, new()
    {
        VisualizationRules<TState, TStimulus> rules = new();
        configureRules(rules);
        return Visualizer<TVisualizerFactory>(rules);
    }

    public IVisualizer Visualizer<TVisualizerFactory>(VisualizationRules<TState, TStimulus>? rules = null) where TVisualizerFactory : IVisualizerFactory<TState, TStimulus>, new()
    {
        rules ??= new VisualizationRules<TState, TStimulus>();
        var factory = new TVisualizerFactory();
        return factory.CreateVisualizer(rules,
            _initialState,
            _stateMap,
            _enterActionRegistry,
            _leaveActionRegistry,
            _guardRegistry);
    }
    
    public TStateMachine  Build()
    {
        return _factory.Create(_initialState, _enterActionRegistry, _leaveActionRegistry, _stateMap, _guardRegistry, _history);
    }
}