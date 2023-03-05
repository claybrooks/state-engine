namespace StateEngine.Tests.Stubs;

public class StubTransitionGuard<TState, TStimulus> : ITransitionGuard<TState, TStimulus>
    where TState : struct where TStimulus : struct
{
    private readonly bool _shouldTransition;
    
    public StubTransitionGuard() : this(false)
    {

    }

    public StubTransitionGuard(bool shouldTransition)
    {
        _shouldTransition = shouldTransition;
    }

    public Task<bool> CheckAsync(ITransition<TState, TStimulus> transition)
    {
        return Task.FromResult(_shouldTransition);
    }
}