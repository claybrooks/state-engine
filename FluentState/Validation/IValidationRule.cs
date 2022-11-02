using FluentState.MachineParts;

namespace FluentState.Validation;

public interface IValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public IValidationResult Run(TState initialState,
        IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation,
        IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IStateGuardValidation<TState, TStimulus> stateGuardValidation);
}
