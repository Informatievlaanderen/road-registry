namespace RoadRegistry.BackOffice.Exceptions;

using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;

public sealed class DutchValidationException : ValidationException
{
    public DutchValidationException(IEnumerable<ValidationFailure> errors)
        : base(errors)
    {
    }
}
