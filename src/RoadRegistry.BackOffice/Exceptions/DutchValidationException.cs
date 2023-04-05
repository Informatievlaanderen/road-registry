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

    public DutchValidationException(string message) : base(message)
    {
    }

    public DutchValidationException(string message, IEnumerable<ValidationFailure> errors) : base(message, errors)
    {
    }

    public DutchValidationException(string message, IEnumerable<ValidationFailure> errors, bool appendDefaultMessage) : base(message, errors, appendDefaultMessage)
    {
    }

    public DutchValidationException(IEnumerable<ValidationFailure> errors) : base(errors)
    {
    }
}
