using System;
using System.Collections.Generic;

namespace RoadRegistry.BackOffice.Exceptions
{
    using FluentValidation;
    using FluentValidation.Results;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class DutchValidationException : ValidationException
    {
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

        public DutchValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
