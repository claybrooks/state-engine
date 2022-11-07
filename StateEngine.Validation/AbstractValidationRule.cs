namespace StateEngine.Validation;

public abstract class AbstractValidationRule<TState, TStimulus> : IValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    protected List<IValidationError<TState, TStimulus>> Errors = new();

    public abstract IValidationResult<TState, TStimulus> Run(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        ITransitionActionRegistryValidation<TState, TStimulus> enterRegistryValidation, ITransitionActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        ITransitionGuardRegistryValidation<TState, TStimulus> guardRegistryValidation);

    public IValidationResult<TState, TStimulus> Result => new ValidationResult<TState, TStimulus> {Errors = Errors};
}