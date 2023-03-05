namespace StateEngine.Tests.Stubs;

public class StubTransitionAction<TState, TStimulus> : ITransitionAction<TState, TStimulus>
    where TState : struct where TStimulus : struct
{
    public string Id => string.Empty;
    public Task OnTransition(ITransition<TState, TStimulus> transition)
    {
        return Task.CompletedTask;
    }
}