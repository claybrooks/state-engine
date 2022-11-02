using FluentState.MachineParts;
using System.Collections.Generic;

namespace FluentState.Validation;

public interface IValidator<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    IValidationResult Validate(
        IEnumerable<IValidationRule<TState, TStimulus>> rules,
        TState initialState,
        IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation,
        IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IStateGuardValidation<TState, TStimulus> stateGuardValidation);
}

public class Validator<TState, TStimulus> : IValidator<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public IValidationResult Validate(
        IEnumerable<IValidationRule<TState, TStimulus>> rules,
        TState initialState,
        IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation,
        IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IStateGuardValidation<TState, TStimulus> stateGuardValidation)
    {
        var errors = new List<IValidationError>();
        var warnings = new List<IValidationWarning>();

        foreach (var rule in rules)
        {
            var result = rule.Run(
                initialState,
                stateMapValidation,
                enterRegistryValidation,
                leaveRegistryValidation,
                stateGuardValidation);

            errors.AddRange(result.Errors);
            warnings.AddRange(result.Warnings);
        }

        return new ValidationResult
        {
            Errors = errors,
            Warnings = warnings
        };
    }
}