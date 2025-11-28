namespace RoadRegistry.BackOffice.Core;

using CommandHandling;
using CommandHandling.Actions.ChangeRoadNetwork.ValueObjects;
using Extensions;
using FluentValidation;
using Messages;
using RoadRegistry.RoadNetwork.ValueObjects;
using ValueObjects.ProblemCodes;

public class RequestedRoadSegmentWidthAttributeValidator : AbstractValidator<RequestedRoadSegmentWidthAttribute>
{
    public RequestedRoadSegmentWidthAttributeValidator()
    {
        RuleFor(c => c.AttributeId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.FromPosition).GreaterThanOrEqualTo(0.0m);
        RuleFor(c => c.ToPosition).GreaterThan(_ => _.FromPosition);
        RuleFor(c => c.Width)
            .Must(RoadSegmentWidth.Accepts)
            .WithProblemCode(ProblemCode.RoadSegment.Width.NotValid);
    }
}

public class RequestedRoadSegmentOutlinedWidthAttributeValidator : RequestedRoadSegmentWidthAttributeValidator
{
    public RequestedRoadSegmentOutlinedWidthAttributeValidator()
    {
        RuleFor(c => c.Width)
            .Must(value => RoadSegmentWidth.Accepts(value) && new RoadSegmentWidth(value).IsValidForEdit())
            .WithProblemCode(ProblemCode.RoadSegment.Width.NotValid);
    }
}
