using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentState;

public interface IGuardRegistryValidation<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    IReadOnlyList<ITransition<TState, TStimulus>> GuardTransitions { get; }
}

public interface ITransitionGuard<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    Task<bool> Check(ITransition<TState, TStimulus> transition);
}

public interface IGuardRegistry<TState, TStimulus> where TState : struct where TStimulus : struct
{
    bool Register(ITransition<TState, TStimulus> transition, Func<ITransition<TState, TStimulus>, bool> guard);
    bool Register<TGuard>(ITransition<TState, TStimulus> transition) where TGuard : ITransitionGuard<TState, TStimulus>, new();
    bool Register(ITransition<TState, TStimulus> transition, ITransitionGuard<TState, TStimulus> transitionGuard);
    Task<bool> CheckTransition(ITransition<TState, TStimulus> transition);
}

internal sealed class DelegateTransitionGuard<TState, TStimulus> : ITransitionGuard<TState, TStimulus>
    where TState : struct where TStimulus : struct
{
    private readonly Func<ITransition<TState, TStimulus>, bool> _delegate;

    public DelegateTransitionGuard(Func<ITransition<TState, TStimulus>, bool> @delegate)
    {
        _delegate = @delegate;
    }

    public Task<bool> Check(ITransition<TState, TStimulus> transition)
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

    public async Task<bool> Check(ITransition<TState, TStimulus> transition)
    {
        return await _delegate(transition);
    }
}

internal sealed class GuardRegistry<TState, TStimulus> : IGuardRegistry<TState, TStimulus>, IGuardRegistryValidation<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    private readonly Dictionary<ITransition<TState, TStimulus>, ITransitionGuard<TState, TStimulus>> _stateTransitionGuards = new(new TransitionComparer<TState, TStimulus>());

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
        return _stateTransitionGuards.TryAdd(transition, transitionGuard);
    }

    public async Task<bool> CheckTransition(ITransition<TState, TStimulus> transition)
    {
        return !_stateTransitionGuards.TryGetValue(transition, out var guard) || await guard.Check(transition);
    }

    public IReadOnlyList<ITransition<TState, TStimulus>> GuardTransitions => _stateTransitionGuards.Keys.ToList();
}