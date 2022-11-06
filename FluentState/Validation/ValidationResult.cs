using System;
using System.Collections.Generic;

namespace FluentState;

public interface IValidationError<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    string Reason { get; }
    IEnumerable<TState> ErrorStates { get; }
    IEnumerable<ITransition<TState, TStimulus>> ErrorTransitions { get;}
}

public interface IValidationResult<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public IReadOnlyList<IValidationError<TState, TStimulus>> Errors { get; }
}

public sealed class ValidationError<TState, TStimulus> : IValidationError<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public string Reason { get; init; } = string.Empty;
    public IEnumerable<TState> ErrorStates { get; init; } = Array.Empty<TState>();
    public IEnumerable<ITransition<TState, TStimulus>> ErrorTransitions { get; init; } = Array.Empty<ITransition<TState, TStimulus>>();
}

public sealed class ValidationResult<TState, TStimulus> : IValidationResult<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public IReadOnlyList<IValidationError<TState, TStimulus>> Errors { get; set; } = Array.Empty<IValidationError<TState, TStimulus>>();
}
