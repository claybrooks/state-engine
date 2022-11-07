namespace StateEngine.Validation;

public sealed class ValidatorFactory<TState, TStimulus> : IValidatorFactory<TState, TStimulus>
    where TState : struct
    where TStimulus : struct

{
    public IValidator<TState, TStimulus> Create(IEnumerable<IValidationRule<TState, TStimulus>> rules, TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        ITransitionActionRegistryValidation<TState, TStimulus> enterRegistryValidation, ITransitionActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        ITransitionGuardRegistryValidation<TState, TStimulus> guardRegistryValidation)
    {
        return new Validator<TState, TStimulus>(rules, initialState, stateMapValidation, enterRegistryValidation, leaveRegistryValidation, guardRegistryValidation);
    }
}

internal sealed class Validator<TState, TStimulus> : IValidator<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    private readonly IEnumerable<IValidationRule<TState, TStimulus>> _rules;
    private readonly TState _initialState;
    private readonly IStateMapValidation<TState, TStimulus> _stateMapValidation;
    private readonly ITransitionActionRegistryValidation<TState, TStimulus> _enterRegistryValidation;
    private readonly ITransitionActionRegistryValidation<TState, TStimulus> _leaveRegistryValidation;
    private readonly ITransitionGuardRegistryValidation<TState, TStimulus> _guardRegistryValidation;

    public Validator(IEnumerable<IValidationRule<TState, TStimulus>> rules, TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation, ITransitionActionRegistryValidation<TState, TStimulus> enterRegistryValidation, ITransitionActionRegistryValidation<TState, TStimulus> leaveRegistryValidation, ITransitionGuardRegistryValidation<TState, TStimulus> guardRegistryValidation)
    {
        this._rules = rules;
        this._initialState = initialState;
        this._stateMapValidation = stateMapValidation;
        this._enterRegistryValidation = enterRegistryValidation;
        this._leaveRegistryValidation = leaveRegistryValidation;
        this._guardRegistryValidation = guardRegistryValidation;
    }

    public IValidationResult<TState, TStimulus> Validate()
    {
        var errors = new List<IValidationError<TState, TStimulus>>();

        foreach (var rule in _rules)
        {
            var result = rule.Run(
                _initialState,
                _stateMapValidation,
                _enterRegistryValidation,
                _leaveRegistryValidation,
                _guardRegistryValidation);

            errors.AddRange(result.Errors);
        }

        return new ValidationResult<TState, TStimulus>
        {
            Errors = errors
        };
    }
}
