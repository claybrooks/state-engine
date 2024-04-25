namespace StateEngine;

public interface ITransitionGuardRegistryValidation<out TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    IReadOnlyList<ITransition<TState, TStimulus>> GuardedTransitions { get; }
    IReadOnlyList<TState> GuardedEntry { get; }
    IReadOnlyList<TState> GuardedLeave { get; }
}

public interface ITransitionGuard<in TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    Task<bool> CheckAsync(ITransition<TState, TStimulus> transition);
}

public interface ITransitionGuardRegistry<TState, TStimulus> where TState : struct where TStimulus : struct
{
    bool Register(ITransition<TState, TStimulus> transition, Func<ITransition<TState, TStimulus>, bool> guard);
    bool Register<TGuard>(ITransition<TState, TStimulus> transition) where TGuard : ITransitionGuard<TState, TStimulus>, new();
    bool Register(ITransition<TState, TStimulus> transition, ITransitionGuard<TState, TStimulus> transitionGuard);
    bool RegisterEnter(TState state, Func<ITransition<TState, TStimulus>, bool> guard);
    bool RegisterEnter<TGuard>(TState state) where TGuard : ITransitionGuard<TState, TStimulus>, new();
    bool RegisterEnter(TState state, ITransitionGuard<TState, TStimulus> transitionGuard);
    bool RegisterLeave(TState state, Func<ITransition<TState, TStimulus>, bool> guard);
    bool RegisterLeave<TGuard>(TState state) where TGuard : ITransitionGuard<TState, TStimulus>, new();
    bool RegisterLeave(TState state, ITransitionGuard<TState, TStimulus> transitionGuard);
    Task<bool> CheckTransitionAsync(ITransition<TState, TStimulus> transition);
    Task<bool> CheckLeaveAsync(ITransition<TState, TStimulus> transition);
    Task<bool> CheckEnterAsync(ITransition<TState, TStimulus> transition);
}

internal sealed class DelegateTransitionGuard<TState, TStimulus> : ITransitionGuard<TState, TStimulus>
    where TState : struct where TStimulus : struct
{
    private readonly Func<ITransition<TState, TStimulus>, bool> _delegate;

    public DelegateTransitionGuard(Func<ITransition<TState, TStimulus>, bool> @delegate)
    {
        _delegate = @delegate;
    }

    public Task<bool> CheckAsync(ITransition<TState, TStimulus> transition)
    {
        return Task.FromResult(_delegate(transition));
    }
}

internal sealed class AsyncDelegateTransitionGuard<TState, TStimulus> : ITransitionGuard<TState, TStimulus>
    where TState : struct where TStimulus : struct
{
    private readonly Func<ITransition<TState, TStimulus>, Task<bool>> _delegate;

    public AsyncDelegateTransitionGuard(Func<ITransition<TState, TStimulus>, Task<bool>> @delegate)
    {
        _delegate = @delegate;
    }

    public async Task<bool> CheckAsync(ITransition<TState, TStimulus> transition)
    {
        return await _delegate(transition);
    }
}

public sealed class GuardRegistry<TState, TStimulus> : ITransitionGuardRegistry<TState, TStimulus>, ITransitionGuardRegistryValidation<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    private readonly Dictionary<ITransition<TState, TStimulus>, List<ITransitionGuard<TState, TStimulus>>> _stateTransitionGuards = new(new TransitionComparer<TState, TStimulus>());
    private readonly Dictionary<TState, List<ITransitionGuard<TState, TStimulus>>> _stateEnterGuards = [];
    private readonly Dictionary<TState, List<ITransitionGuard<TState, TStimulus>>> _stateLeaveGuards = [];

    public bool Register(ITransition<TState, TStimulus> transition, Func<ITransition<TState, TStimulus>, bool> guard)
    {
        return Register(transition, new DelegateTransitionGuard<TState, TStimulus>(guard));
    }

    public bool Register<TGuard>(ITransition<TState, TStimulus> transition) where TGuard : ITransitionGuard<TState, TStimulus>, new()
    {
        return Register(transition, new TGuard());
    }

    public bool Register(ITransition<TState, TStimulus> transition, ITransitionGuard<TState, TStimulus> transitionGuard)
    {
        if (!_stateTransitionGuards.TryGetValue(transition, out var list))
        {
            list = [];
            _stateTransitionGuards[transition] = list;
        }
        list.Add(transitionGuard);
        return true;
    }

    public bool RegisterEnter(TState state, Func<ITransition<TState, TStimulus>, bool> guard)
    {
        return RegisterEnter(state, new DelegateTransitionGuard<TState, TStimulus>(guard));
    }

    public bool RegisterEnter<TGuard>(TState state) where TGuard : ITransitionGuard<TState, TStimulus>, new()
    {
        return RegisterEnter(state, new TGuard());
    }

    public bool RegisterEnter(TState state, ITransitionGuard<TState, TStimulus> transitionGuard)
    {
        if (!_stateEnterGuards.TryGetValue(state, out var list))
        {
            list = [];
            _stateEnterGuards[state] = list;
        }
        list.Add(transitionGuard);
        return true;
    }

    public bool RegisterLeave(TState state, Func<ITransition<TState, TStimulus>, bool> guard)
    {
        return RegisterLeave(state, new DelegateTransitionGuard<TState, TStimulus>(guard));
    }

    public bool RegisterLeave<TGuard>(TState state) where TGuard : ITransitionGuard<TState, TStimulus>, new()
    {
        return RegisterLeave(state, new TGuard());
    }

    public bool RegisterLeave(TState state, ITransitionGuard<TState, TStimulus> transitionGuard)
    {
        if (!_stateLeaveGuards.TryGetValue(state, out var list))
        {
            list = [];
            _stateLeaveGuards[state] = list;
        }
        list.Add(transitionGuard);
        return true;
    }

    public async Task<bool> CheckTransitionAsync(ITransition<TState, TStimulus> transition)
    {
        if (_stateTransitionGuards.TryGetValue(transition, out var guards))
        {
            foreach (var guard in guards)
            {
                if (!await guard.CheckAsync(transition))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public async Task<bool> CheckLeaveAsync(ITransition<TState, TStimulus> transition)
    {
        if (_stateLeaveGuards.TryGetValue(transition.From, out var guards))
        {
            foreach (var guard in guards)
            {
                if (!await guard.CheckAsync(transition))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public async Task<bool> CheckEnterAsync(ITransition<TState, TStimulus> transition)
    {
        if (_stateEnterGuards.TryGetValue(transition.To, out var guards))
        {
            foreach (var guard in guards)
            {
                if (!await guard.CheckAsync(transition))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public IReadOnlyList<ITransition<TState, TStimulus>> GuardedTransitions => _stateTransitionGuards.Keys.ToList();
    public IReadOnlyList<TState> GuardedEntry => _stateEnterGuards.Keys.ToList();
    public IReadOnlyList<TState> GuardedLeave => _stateLeaveGuards.Keys.ToList();
}