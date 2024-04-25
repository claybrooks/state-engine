namespace StateEngine;

public interface ITransitionActionRegistryValidation<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    IEnumerable<string> GlobalActions { get; }
    IReadOnlyDictionary<TState, IEnumerable<string>> StateWideActions { get; }
    IReadOnlyDictionary<ITransition<TState, TStimulus>, IEnumerable<string>> ActionsOnTransition { get; }
}

public interface ITransitionAction<in TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    string Id { get; }
    Task OnTransition(ITransition<TState, TStimulus> transition);
}

public interface ITransitionActionRegistry<TState, TStimulus> where TState : struct where TStimulus : struct
{
    void Register(ITransitionAction<TState, TStimulus> transitionAction);
    void Register(TState state, ITransitionAction<TState, TStimulus> transitionAction);
    void Register(TState from, TState to, TStimulus when, ITransitionAction<TState, TStimulus> transitionAction);
    void Register(ITransition<TState, TStimulus> transition, ITransitionAction<TState, TStimulus> transitionAction);

    void Trigger(TState state, ITransition<TState, TStimulus> transition);
}

internal abstract class AbstractDelegateTransitionAction<TState, TStimulus> : ITransitionAction<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    private readonly string _filePath;
    private readonly string _caller;
    private readonly int _lineNumber;
    private readonly string? _idOverride;
    protected AbstractDelegateTransitionAction(string? idOverride, string filePath, string caller, int lineNumber)
    {
        _idOverride = idOverride;
        _filePath = filePath;
        _caller = caller;
        _lineNumber = lineNumber;
    }

    public string Id => _idOverride ?? $"{Path.GetFileName(_filePath)}.{_caller}:{_lineNumber}";

    public abstract Task OnTransition(ITransition<TState, TStimulus> transition);
}

internal sealed class DelegateTransitionAction<TState, TStimulus> : AbstractDelegateTransitionAction<TState, TStimulus>
    where TState : struct where TStimulus : struct
{
    private readonly Action<ITransition<TState, TStimulus>> _delegate;
    public DelegateTransitionAction(Action<ITransition<TState, TStimulus>> @delegate,
        string? idOverride,
        string filePath,
        string caller,
        int lineNumber) : base(idOverride,
        filePath,
        caller,
        lineNumber)
    {
        _delegate = @delegate;
    }

    public override Task OnTransition(ITransition<TState, TStimulus> transition)
    {
        _delegate(transition);
        return Task.CompletedTask;
    }
}

internal sealed class AsyncDelegateTransitionAction<TState, TStimulus> : AbstractDelegateTransitionAction<TState, TStimulus>
    where TState : struct where TStimulus : struct
{
    private readonly Func<ITransition<TState, TStimulus>, Task> _delegate;

    public AsyncDelegateTransitionAction(Func<ITransition<TState, TStimulus>, Task> @delegate,
        string? idOverride,
        string filePath,
        string caller,
        int lineNumber) : base(idOverride,
        filePath,
        caller,
        lineNumber)
    {
        _delegate = @delegate;
    }

    public override async Task OnTransition(ITransition<TState, TStimulus> transition)
    {
        await _delegate(transition);
    }
}

public sealed class TransitionAction<TState, TStimulus> : ITransitionActionRegistry<TState, TStimulus>, ITransitionActionRegistryValidation<TState, TStimulus>
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

    public IEnumerable<string> GlobalActions => _globalActions.Select(ta => ta.Id).ToList();

    public IReadOnlyDictionary<TState, IEnumerable<string>> StateWideActions =>
        _stateActions.ToDictionary(
            kvp => kvp.Key, kvp => kvp.Value.Select(ta => ta.Id));

    public IReadOnlyDictionary<ITransition<TState, TStimulus>, IEnumerable<string>> ActionsOnTransition =>
        _transitionActions.ToDictionary(
            kvp => kvp.Key, kvp => kvp.Value.Select(t => t.Id), new TransitionComparer<TState, TStimulus>());
}
