using System.Collections.Generic;

namespace FluentState;


public abstract class AbstractValidationRule<TState, TStimulus> : IValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    protected List<IValidationError> Errors = new();
    protected List<IValidationWarning> Warnings = new();

    public abstract IValidationResult Run(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation, IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IGuardRegistryValidation<TState, TStimulus> guardRegistryValidation);

    public IValidationResult Result => new ValidationResult {Errors = Errors, Warnings = Warnings};
}