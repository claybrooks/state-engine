using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentState;

public interface IActionRegistryValidation<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    IReadOnlyList<ITransition<TState, TStimulus>> TransitionActionTransitions { get; }
}

public interface ITransitionAction<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    Task OnTransition(ITransition<TState, TStimulus> transition);
}

public interface IActionRegistry<TState, TStimulus> where TState : struct where TStimulus : struct
{
    void Register(ITransitionAction<TState, TStimulus> transitionAction);
    void Register(TState state, ITransitionAction<TState, TStimulus> transitionAction);
    void Register(TState from, TState to, TStimulus when, ITransitionAction<TState, TStimulus> transitionAction);
    void Register(ITransition<TState, TStimulus> transition, ITransitionAction<TState, TStimulus> transitionAction);

    void Trigger(TState state, ITransition<TState, TStimulus> transition);
}

internal sealed class DelegateTransitionAction<TState, TStimulus> : ITransitionAction<TState, TStimulus>
    where TState : struct where TStimulus : struct
{
    private readonly Action<ITransition<TState, TStimulus>> _delegate;

    public DelegateTransitionAction(Action<ITransition<TState, TStimulus>> @delegate)
    {
        _delegate = @delegate;
    }

    public Task OnTransition(ITransition<TState, TStimulus> transition)
    {
        _delegate(transition);
        return Task.CompletedTask;
    }
}

internal sealed class AsyncDelegateTransitionAction<TState, TStimulus> : ITransitionAction<TState, TStimulus>
    where TState : struct where TStimulus : struct
{
    private readonly Func<ITransition<TState, TStimulus>, Task> _delegate;

    public AsyncDelegateTransitionAction(Func<ITransition<TState, TStimulus>, Task> @delegate)
    {
        _delegate = @delegate;
    }

    public async Task OnTransition(ITransition<TState, TStimulus> transition)
    {
        await _delegate(transition);
    }
}

internal sealed class ActionRegistry<TState, TStimulus> : IActionRegistry<TState, TStimulus>, IActionRegistryValidation<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    private readonly List<ITransitionAction<TState, TStimulus>> _globalActions = new();
    private readonly Dictionary<TState, List<ITransitionAction<TState, TStimulus>>> _stateActions = new();
    private readonly Dictionary<ITransition<TState, TStimulus>, List<ITransitionAction<TState, TStimulus>>> _transitionActions = new(new TransitionComparer<TState, TStimulus>());

    public void Register(ITransitionAction<TState, TStimulus> transitionAction)
    {
        _globalActions.Add(transitionAction);
    }

    public void Register(TState state, ITransitionAction<TState, TStimulus> transitionAction)
    {
        if (!_stateActions.ContainsKey(state))
        {
            _stateActions[state] = new List<ITransitionAction<TState, TStimulus>>();
        }

        _stateActions[state].Add(transitionAction);
    }

    public void Register(TState from, TState to, TStimulus when, ITransitionAction<TState, TStimulus> transitionAction)
    {
        Register(new Transition<TState, TStimulus>{From = from, To = to, Reason = when}, transitionAction);
    }

    public void Register(ITransition<TState, TStimulus> transition, ITransitionAction<TState, TStimulus> transitionAction)
    {
        if (!_transitionActions.ContainsKey(transition))
        {
            _transitionActions[transition] = new List<ITransitionAction<TState, TStimulus>>();
        }
        _transitionActions[transition].Add(transitionAction);
    }

    public void Trigger(TState state, ITransition<TState, TStimulus> transition)
    {
        DoTriggerActions(_globalActions, transition);
        DoTriggerActions(_stateActions, state, transition);
        DoTriggerActions(_transitionActions, transition, transition);
    }

    private static void DoTriggerActions<TKey>(IReadOnlyDictionary<TKey, List<ITransitionAction<TState, TStimulus>>> actionMap, TKey key, ITransition<TState, TStimulus> transition)
        where TKey : notnull
    {
        if (actionMap.TryGetValue(key, out var actions))
        {
            DoTriggerActions(actions, transition);
        }
    }

    private static void DoTriggerActions(IEnumerable<ITransitionAction<TState, TStimulus>> actions, ITransition<TState, TStimulus> transition)
    {
        foreach (var action in actions)
        {
            action.OnTransition(transition);
        }
    }

    public IReadOnlyList<ITransition<TState, TStimulus>> TransitionActionTransitions => _transitionActions.Keys.ToList();
}