namespace RoadRegistry.BackOffice.Exceptions;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using FluentValidation;
using FluentValidation.Results;

[Serializable]
public sealed class DutchValidationException : ValidationException
{
    private DutchValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public DutchValidationException(IEnumerable<ValidationFailure> errors) : base(errors)
    {
    }
}
