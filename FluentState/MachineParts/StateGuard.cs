using System;
using System.Collections.Generic;

namespace FluentState.MachineParts;

public interface IGuard<TState, TStimulus> where TState : struct where TStimulus : struct
{
    bool Check(Transition<TState, TStimulus> transition);
}

public class DelegateGuard<TState, TStimulus> : IGuard<TState, TStimulus>
    where TState : struct where TStimulus : struct
{
    private readonly Func<Transition<TState, TStimulus>, bool> _delegate;

    public DelegateGuard(Func<Transition<TState, TStimulus>, bool> @delegate)
    {
        _delegate = @delegate;
    }

    public bool Check(Transition<TState, TStimulus> transition)
    {
        return _delegate(transition);
    }
}

public interface IStateGuard<TState, TStimulus> where TState : struct where TStimulus : struct
{
    bool Register(Transition<TState, TStimulus> transition, Func<Transition<TState, TStimulus>, bool> guard);
    bool Register<TGuard>(Transition<TState, TStimulus> transition) where TGuard : IGuard<TState, TStimulus>, new();
    bool Register(Transition<TState, TStimulus> transition, IGuard<TState, TStimulus> guard);
    bool CheckTransition(Transition<TState, TStimulus> transition);
}

public class StateGuard<TState, TStimulus> : IStateGuard<TState, TStimulus> where TState : struct where TStimulus : struct
{
    private readonly Dictionary<Transition<TState, TStimulus>, IGuard<TState, TStimulus>> _stateTransitionGuards = new(new TransitionComparer<TState, TStimulus>());

    public bool Register(Transition<TState, TStimulus> transition, Func<Transition<TState, TStimulus>, bool> guard)
    {
        return Register(transition, new DelegateGuard<TState, TStimulus>(guard));
    }

    public bool Register<TGuard>(Transition<TState, TStimulus> transition) where TGuard : IGuard<TState, TStimulus>, new()
    {
        return Register(transition, new TGuard());
    }

    public bool Register(Transition<TState, TStimulus> transition, IGuard<TState, TStimulus> guard)
    {
        return _stateTransitionGuards.TryAdd(transition, guard);
    }

    public bool CheckTransition(Transition<TState, TStimulus> transition)
    {
        return !_stateTransitionGuards.TryGetValue(transition, out var guard) || guard.Check(transition);
    }
}