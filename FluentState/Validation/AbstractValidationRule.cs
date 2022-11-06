using System.Collections.Generic;

namespace FluentState;


public abstract class AbstractValidationRule<TState, TStimulus> : IValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    protected List<IValidationError<TState, TStimulus>> Errors = new();

    public abstract IValidationResult<TState, TStimulus> Run(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation, IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IGuardRegistryValidation<TState, TStimulus> guardRegistryValidation);

    public IValidationResult<TState, TStimulus> Result => new ValidationResult<TState, TStimulus> {Errors = Errors};
}