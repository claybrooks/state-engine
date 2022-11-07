namespace StateEngine;

public interface IValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public IValidationResult<TState, TStimulus> Run(TState initialState,
        IStateMapValidation<TState, TStimulus> stateMapValidation,
        ITransitionActionRegistryValidation<TState, TStimulus> enterRegistryValidation,
        ITransitionActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        ITransitionGuardRegistryValidation<TState, TStimulus> guardRegistryValidation);
}

public interface IValidatorFactory<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    IValidator<TState, TStimulus> Create(IEnumerable<IValidationRule<TState, TStimulus>> rules,
        TState initialState,
        IStateMapValidation<TState, TStimulus> stateMapValidation,
        ITransitionActionRegistryValidation<TState, TStimulus> enterRegistryValidation,
        ITransitionActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        ITransitionGuardRegistryValidation<TState, TStimulus> guardRegistryValidation);
}

public interface IValidator<out TState, out TStimulus>
    where TState : struct
    where TStimulus : struct
{
    IValidationResult<TState, TStimulus> Validate();
}

public interface IValidationError<out TState, out TStimulus>
    where TState : struct
    where TStimulus : struct
{
    string Reason { get; }
    IEnumerable<TState> ErrorStates { get; }
    IEnumerable<ITransition<TState, TStimulus>> ErrorTransitions { get;}
}

public interface IValidationResult<out TState, out TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public IReadOnlyList<IValidationError<TState, TStimulus>> Errors { get; }
}

public sealed class ValidationError<TState, TStimulus> : IValidationError<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public string Reason { get; set; } = string.Empty;
    public IEnumerable<TState> ErrorStates { get; set; } = Array.Empty<TState>();
    public IEnumerable<ITransition<TState, TStimulus>> ErrorTransitions { get; set; } = Array.Empty<ITransition<TState, TStimulus>>();
}

public sealed class ValidationResult<TState, TStimulus> : IValidationResult<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public IReadOnlyList<IValidationError<TState, TStimulus>> Errors { get; set; } = Array.Empty<IValidationError<TState, TStimulus>>();
}
