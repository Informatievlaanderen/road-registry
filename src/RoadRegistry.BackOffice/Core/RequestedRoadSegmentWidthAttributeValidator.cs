namespace RoadRegistry.BackOffice.Core;

using FluentValidation;
using Messages;

public class RequestedRoadSegmentWidthAttributeValidator : AbstractValidator<RequestedRoadSegmentWidthAttribute>
{
    public RequestedRoadSegmentWidthAttributeValidator()
    {
        RuleFor(c => c.AttributeId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.FromPosition).GreaterThanOrEqualTo(0.0m);
        RuleFor(c => c.ToPosition).GreaterThan(_ => _.FromPosition);
        RuleFor(c => c.Width)
            .InclusiveBetween(0, RoadSegmentWidth.Maximum.ToInt32())
            .When(_ => _.Width != RoadSegmentWidth.Unknown && _.Width != RoadSegmentWidth.NotApplicable, ApplyConditionTo.CurrentValidator);
    }
}
