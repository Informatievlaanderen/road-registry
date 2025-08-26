namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using FluentValidation;
using FluentValidation.Results;

public class InvalidGuidValidationException : ValidationException
{
    public InvalidGuidValidationException(string parameterName)
        : base([
            new ValidationFailure(
                parameterName,
                $"'{parameterName}' path parameter is not a global unique identifier without dashes.")
            {
                ErrorCode = "GuidOngeldig"
            }
        ])
    {
    }
}
