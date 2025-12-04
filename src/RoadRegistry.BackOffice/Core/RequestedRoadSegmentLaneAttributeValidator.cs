namespace RoadRegistry.BackOffice.Core;

using CommandHandling;
using CommandHandling.Actions.ChangeRoadNetwork.ValueObjects;
using Extensions;
using FluentValidation;
using Messages;
using ValueObjects.ProblemCodes;

public class RequestedRoadSegmentLaneAttributeValidator : AbstractValidator<RequestedRoadSegmentLaneAttribute>
{
    public RequestedRoadSegmentLaneAttributeValidator()
    {
        RuleFor(c => c.AttributeId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.FromPosition).GreaterThanOrEqualTo(0.0m);
        RuleFor(c => c.ToPosition).GreaterThan(_ => _.FromPosition);
        RuleFor(c => c.Count)
            .Must(RoadSegmentLaneCount.Accepts)
            .WithProblemCode(ProblemCode.RoadSegment.LaneCount.NotValid);
        RuleFor(c => c.Direction)
            .NotEmpty()
            .Must(RoadSegmentLaneDirection.CanParse)
            .When(c => c.Direction != null, ApplyConditionTo.CurrentValidator)
            .WithProblemCode(ProblemCode.RoadSegment.LaneDirection.NotValid);
    }
}

public class RequestedRoadSegmentOutlinedLaneAttributeValidator : RequestedRoadSegmentLaneAttributeValidator
{
    public RequestedRoadSegmentOutlinedLaneAttributeValidator()
    {
        RuleFor(c => c.Count)
            .Must(x => RoadSegmentLaneCount.Accepts(x) && new RoadSegmentLaneCount(x).IsValidForEdit())
            .WithProblemCode(ProblemCode.RoadSegment.LaneCount.NotValid);
    }
}
