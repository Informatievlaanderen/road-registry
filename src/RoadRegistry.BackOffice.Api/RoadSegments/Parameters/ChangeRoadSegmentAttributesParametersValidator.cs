namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using System.Linq;
using Abstractions.Validation;
using FluentValidation;
using FluentValidation.Results;

public class ChangeRoadSegmentAttributesParametersValidator : AbstractValidator<ChangeRoadSegmentAttributesParameters>
{
    /// <summary>
    /// Determines if validation should occtur and provides a means to modify the context and ValidationResult prior to execution.
    /// If this method returns false, then the ValidationResult is immediately returned from Validate/ValidateAsync.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="result">The result.</param>
    /// <remarks>
    /// The output for validation errors will not be correct if you do not fill in the property name!
    /// You should not use the format noted inside the code below.
    /// It's a valid exception but does not format correctly.
    /// </remarks>
    /// <code>
    /// context.AddFailure(new ValidationFailure
    /// {
    ///     ErrorCode = ValidationErrors.RoadSegment.ChangeAttributesRequestNull.Code,
    ///     ErrorMessage = ValidationErrors.RoadSegment.ChangeAttributesRequestNull.Message
    /// });
    /// </code>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    protected override bool PreValidate(ValidationContext<ChangeRoadSegmentAttributesParameters> context, ValidationResult result)
    {
        if (context.InstanceToValidate is not null && context.InstanceToValidate.Any())
            return true;

        context.AddFailure(new ValidationFailure("request", ValidationErrors.RoadSegment.ChangeAttributesRequestNull.Message)
        {
            ErrorCode = ValidationErrors.RoadSegment.ChangeAttributesRequestNull.Code
        });

        return false;
    }
}
