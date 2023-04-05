namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using Core.ProblemCodes;
using Extensions;
using FluentValidation;

public class RoadSegmentLaneParametersValidator : AbstractValidator<RoadSegmentLaneParameters>
{
    public RoadSegmentLaneParametersValidator()
    {
        RuleFor(x => x.Aantal)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.Lane.IsRequired)
            .GreaterThan(0)
            .WithProblemCode(ProblemCode.RoadSegment.Lane.GreaterThanZero)
            .LessThanOrEqualTo(RoadSegmentLaneCount.Maximum)
            .WithProblemCode(ProblemCode.RoadSegment.Lane.LessThanOrEqualToMaximum);

        RuleFor(x => x.Richting)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.LaneDirection.IsRequired)
            .Must(RoadSegmentLaneDirection.CanParseUsingDutchName)
            .WithProblemCode(ProblemCode.RoadSegment.LaneDirection.NotValid);
    }
}
