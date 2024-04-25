namespace StateEngine.Tests.Stubs;

public class StubTransitionGuardRegistry<TState, TStimulus> : ITransitionGuardRegistry<TState, TStimulus>, ITransitionGuardRegistryValidation<TState, TStimulus>
    where TState : struct where TStimulus : struct
{
    public bool AnythingRegistered = false;
    public ITransition<TState, TStimulus>? LastRegisteredTransition { get; private set; }

    public bool Register(ITransition<TState, TStimulus> transition, Func<ITransition<TState, TStimulus>, bool> guard)
    {
        AnythingRegistered = true;
        LastRegisteredTransition = transition;
        return true;
    }

    public bool Register<TGuard>(ITransition<TState, TStimulus> transition) where TGuard : ITransitionGuard<TState, TStimulus>, new()
    {
        AnythingRegistered = true;
        LastRegisteredTransition = transition;
        return true;
    }

    public bool Register(ITransition<TState, TStimulus> transition, ITransitionGuard<TState, TStimulus> transitionGuard)
    {
        AnythingRegistered = true;
        LastRegisteredTransition = transition;
        return true;
    }

    public bool RegisterEnter(TState state, Func<ITransition<TState, TStimulus>, bool> guard)
    {
        return false;
    }

    public bool RegisterEnter<TGuard>(TState state) where TGuard : ITransitionGuard<TState, TStimulus>, new()
    {
        return false;
    }

    public bool RegisterEnter(TState state, ITransitionGuard<TState, TStimulus> transitionGuard)
    {
        return false;
    }

    public bool RegisterLeave(TState state, Func<ITransition<TState, TStimulus>, bool> guard)
    {
        return false;
    }

    public bool RegisterLeave<TGuard>(TState state) where TGuard : ITransitionGuard<TState, TStimulus>, new()
    {
        return false;
    }

    public bool RegisterLeave(TState state, ITransitionGuard<TState, TStimulus> transitionGuard)
    {
        return false;
    }

    public Task<bool> CheckTransitionAsync(ITransition<TState, TStimulus> transition)
    {
        return Task.FromResult(false);
    }

    public Task<bool> CheckLeaveAsync(ITransition<TState, TStimulus> transition)
    {
        return Task.FromResult(false);
    }

    public Task<bool> CheckEnterAsync(ITransition<TState, TStimulus> transition)
    {
        return Task.FromResult(false);
    }

    public IReadOnlyList<ITransition<TState, TStimulus>> GuardedTransitions { get; } = new List<ITransition<TState, TStimulus>>();
    public IReadOnlyList<TState> GuardedEntry { get; } = new List<TState>();
    public IReadOnlyList<TState> GuardedLeave { get; } = new List<TState>();
}