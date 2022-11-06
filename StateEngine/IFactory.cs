namespace StateEngine;

public interface IStateMachineFactory<out TStateMachine, TState, TStimulus>
    where TStateMachine : IStateMachine<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    TStateMachine Create(TState initialState,
        IActionRegistry<TState, TStimulus> enterActions,
        IActionRegistry<TState, TStimulus> leaveActions,
        IStateMap<TState, TStimulus> stateTransitions,
        IGuardRegistry<TState, TStimulus> guardRegistry,
        IStateMachineHistory<TState, TStimulus> history);
}