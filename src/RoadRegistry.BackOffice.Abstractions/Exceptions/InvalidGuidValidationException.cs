namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;
using FluentValidation;
using FluentValidation.Results;

[Serializable]
public class InvalidGuidValidationException : ValidationException
{
    public InvalidGuidValidationException(string parameterName)
        : base(new[]
        {
            new ValidationFailure(
                parameterName,
                $"'{parameterName}' path parameter is not a global unique identifier without dashes.")
        })
    {
    }
    
    protected InvalidGuidValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
