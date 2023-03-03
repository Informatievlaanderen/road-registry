namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using FluentValidation;
using RoadRegistry.BackOffice.Abstractions.Validation;

public class RoadSegmentLaneParametersValidator : AbstractValidator<RoadSegmentLaneParameters>
{
    public RoadSegmentLaneParametersValidator()
    {
        RuleFor(x => x.Aantal)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0)
            .WithErrorCode(ValidationErrors.RoadSegment.Lane.GreaterThanZero.Code)
            .WithMessage(ValidationErrors.RoadSegment.Lane.GreaterThanZero.Message);
    }
}
