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

        RuleFor(x => x.Richting)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(ValidationErrors.RoadSegment.LaneDirection.IsRequired.Code)
            .WithMessage(ValidationErrors.RoadSegment.LaneDirection.IsRequired.Message)
            .Must(RoadSegmentLaneDirection.CanParseUsingDutchName)
            .WithErrorCode(ValidationErrors.RoadSegment.LaneDirection.NotParsed.Code)
            .WithMessage(x => ValidationErrors.RoadSegment.LaneDirection.NotParsed.Message(x.Richting));
    }
}
