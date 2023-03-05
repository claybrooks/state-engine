namespace StateEngine.Tests.Stubs;

public class StubEngineFactory<TState, TStimulus> : IStateEngineFactory<TState, TStimulus>
    where TState : struct where TStimulus : struct
{
    public IStateEngine<TState, TStimulus> Create(TState initialState, ITransitionActionRegistry<TState, TStimulus> enterActions, ITransitionActionRegistry<TState, TStimulus> leaveActions,
        IStateMap<TState, TStimulus> stateTransitions, ITransitionGuardRegistry<TState, TStimulus> guardRegistry, IHistory<TState, TStimulus> history)
    {
        return new StubEngine<TState, TStimulus>(initialState, enterActions, leaveActions, stateTransitions, guardRegistry, history);
    }
}