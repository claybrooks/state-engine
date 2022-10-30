using System;
using System.Collections.Generic;

namespace FluentState.MachineParts;

public interface IAction<TState, TStimulus> where TState : struct where TStimulus : struct
{
    void OnTransition(Transition<TState, TStimulus> transition);
}

public class DelegateAction<TState, TStimulus> : IAction<TState, TStimulus>
    where TState : struct where TStimulus : struct
{
    private readonly Action<Transition<TState, TStimulus>> _delegate;

    public DelegateAction(Action<Transition<TState, TStimulus>> @delegate)
    {
        _delegate = @delegate;
    }

    public void OnTransition(Transition<TState, TStimulus> transition)
    {
        _delegate(transition);
    }
}

public interface IActionRegistry<TState, TStimulus> where TState : struct where TStimulus : struct
{
    void Register(IAction<TState, TStimulus> action);
    void Register(TState state, IAction<TState, TStimulus> action);
    void Register(TState from, TState to, TStimulus when, IAction<TState, TStimulus> action);
    void Register(Transition<TState, TStimulus> transition, IAction<TState, TStimulus> action);

    void Trigger(TState stateKey, Transition<TState, TStimulus> transition);
}

public class ActionRegistry<TState, TStimulus> : IActionRegistry<TState, TStimulus> where TState : struct
    where TStimulus : struct
{
    private readonly List<IAction<TState, TStimulus>> _globalActions = new();
    private readonly Dictionary<TState, List<IAction<TState, TStimulus>>> _stateWideActions = new();
    private readonly Dictionary<Transition<TState, TStimulus>, List<IAction<TState, TStimulus>>> _statePairAndStimulusActions = new(new TransitionComparer<TState, TStimulus>());

    public void Register(IAction<TState, TStimulus> action)
    {
        _globalActions.Add(action);
    }

    public void Register(TState state, IAction<TState, TStimulus> action)
    {
        if (!_stateWideActions.ContainsKey(state))
        {
            _stateWideActions[state] = new List<IAction<TState, TStimulus>>();
        }

        _stateWideActions[state].Add(action);
    }

    public void Register(TState from, TState to, TStimulus when, IAction<TState, TStimulus> action)
    {
        Register(new Transition<TState, TStimulus>{From = from, To = to, Reason = when}, action);
    }

    public void Register(Transition<TState, TStimulus> transition, IAction<TState, TStimulus> action)
    {
        if (!_statePairAndStimulusActions.ContainsKey(transition))
        {
            _statePairAndStimulusActions[transition] = new List<IAction<TState, TStimulus>>();
        }
        _statePairAndStimulusActions[transition].Add(action);
    }

    public void Trigger(TState stateKey, Transition<TState, TStimulus> transition)
    {
        DoTriggerActions(_globalActions, transition);
        DoTriggerActions(_stateWideActions, stateKey, transition);
        DoTriggerActions(_statePairAndStimulusActions, transition, transition);
    }

    private static void DoTriggerActions<TKey>(IReadOnlyDictionary<TKey, List<IAction<TState, TStimulus>>> actionMap, TKey key, Transition<TState, TStimulus> transition)
        where TKey : notnull
    {
        if (actionMap.TryGetValue(key, out var actions))
        {
            DoTriggerActions(actions, transition);
        }
    }

    private static void DoTriggerActions(IEnumerable<IAction<TState, TStimulus>> actions, Transition<TState, TStimulus> transition)
    {
        foreach (var action in actions)
        {
            action.OnTransition(transition);
        }
    }
}