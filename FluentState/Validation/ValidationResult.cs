using System;
using System.Collections.Generic;

namespace FluentState;

public interface IValidationError
{
    string Reason { get; }
}

public interface IValidationWarning
{
    string Reason { get; }
}

public interface IValidationResult
{
    public IReadOnlyList<IValidationError> Errors { get; }
    public IReadOnlyList<IValidationWarning> Warnings { get; }
}

public sealed class ValidationError : IValidationError
{
    public string Reason { get; set; } = string.Empty;
}

public sealed class ValidationWarning : IValidationWarning
{
    public string Reason { get; set; } = string.Empty;
}

public sealed class ValidationResult : IValidationResult
{
    public IReadOnlyList<IValidationError> Errors { get; set; } = Array.Empty<IValidationError>();
    public IReadOnlyList<IValidationWarning> Warnings { get; set; } = Array.Empty<IValidationWarning>();
}
