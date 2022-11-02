using FluentState.MachineParts;
using System.Collections.Generic;

namespace FluentState.Validation;


public abstract class AbstractValidationRule<TState, TStimulus> : IValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    protected List<ValidationError> Errors = new();
    protected List<ValidationWarning> Warnings = new();

    public abstract IValidationResult Run(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation, IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IStateGuardValidation<TState, TStimulus> stateGuardValidation);

    protected ValidationResult Result => new() {Errors = Errors, Warnings = Warnings};
}