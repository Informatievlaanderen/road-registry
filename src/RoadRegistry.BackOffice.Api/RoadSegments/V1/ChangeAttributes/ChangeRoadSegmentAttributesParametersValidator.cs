namespace RoadRegistry.BackOffice.Api.RoadSegments.V1.ChangeAttributes;

using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using RoadRegistry.ValueObjects.ProblemCodes;

public class ChangeRoadSegmentAttributesParametersValidator : AbstractValidator<ChangeRoadSegmentAttributesParameters>
{
    protected override bool PreValidate(ValidationContext<ChangeRoadSegmentAttributesParameters> context, ValidationResult result)
    {
        if (context.InstanceToValidate is not null && context.InstanceToValidate.Any())
        {
            return true;
        }

        context.AddFailure(new ValidationFailure
        {
            PropertyName = "request",
            ErrorCode = ProblemCode.RoadSegment.ChangeAttributesRequestNull
        });

        return false;
    }
}
