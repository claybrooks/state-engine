namespace FluentState;

public interface IValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public IValidationResult<TState, TStimulus> Run(TState initialState,
        IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation,
        IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IGuardRegistryValidation<TState, TStimulus> guardRegistryValidation);
}
