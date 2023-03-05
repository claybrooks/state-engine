namespace StateEngine.Tests.Stubs;

public class StubStateMap<TState, TStimulus> : IStateMap<TState, TStimulus>, IStateMapValidation<TState, TStimulus>
    where TState : struct where TStimulus : struct
{
    public bool AnythingRegistered = false;

    public bool Register(ITransition<TState, TStimulus> transition)
    {
        AnythingRegistered = true;
        return true;
    }

    public bool CheckTransition(TState currentState, TStimulus reason, out TState nextState)
    {
        throw new NotImplementedException();
    }

    public bool HasTopLevelState(TState state)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<TState> TopLevelStates { get; } = new List<TState>();

    public IReadOnlyDictionary<TStimulus, TState> StateTransitions(TState state)
    {
        throw new NotImplementedException();
    }

    public bool IsTransitionRegistered(ITransition<TState, TStimulus> transition)
    {
        throw new NotImplementedException();
    }
}