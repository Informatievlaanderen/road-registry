namespace RoadRegistry.BackOffice.Api.RoadSegmentsOutline.Parameters;

using Abstractions.Validation;
using FluentValidation;

public class RoadSegmentLaneParametersValidator : AbstractValidator<RoadSegmentLaneParameters>
{
    public RoadSegmentLaneParametersValidator()
    {
        RuleFor(x => x.Aantal)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0)
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.Lane.GreaterThanZero.Code)
            .WithMessage(ValidationErrors.RoadSegmentOutline.Lane.GreaterThanZero.Message);
    }
}
