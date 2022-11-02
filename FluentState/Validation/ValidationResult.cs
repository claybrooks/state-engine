using System;
using System.Collections.Generic;

namespace FluentState.Validation;

public interface IValidationError
{
    string Reason { get; }
}

public class ValidationError : IValidationError
{
    public string Reason { get; set; } = string.Empty;
}

public interface IValidationWarning
{
    string Reason { get; }
}

public class ValidationWarning : IValidationWarning
{
    public string Reason { get; set; } = string.Empty;
}

public interface IValidationResult
{
    public IReadOnlyList<IValidationError> Errors { get; }
    public IReadOnlyList<IValidationWarning> Warnings { get; }
}

public class ValidationResult : IValidationResult
{
    public IReadOnlyList<IValidationError> Errors { get; set; } = Array.Empty<IValidationError>();
    public IReadOnlyList<IValidationWarning> Warnings { get; set; } = Array.Empty<IValidationWarning>();
}
