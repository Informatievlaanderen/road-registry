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
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.Lane.GreaterThanZero.Code)
            .WithMessage(ValidationErrors.RoadSegmentOutline.Lane.GreaterThanZero.Message);

        RuleFor(x => x.Richting)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.LaneDirection.IsRequired.Code)
            .WithMessage(ValidationErrors.RoadSegmentOutline.LaneDirection.IsRequired.Message)
            .Must(RoadSegmentLaneDirection.CanParseUsingDutchName)
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.LaneDirection.NotParsed.Code)
            .WithMessage(x => ValidationErrors.RoadSegmentOutline.LaneDirection.NotParsed.Message(x.Richting));
    }
}
