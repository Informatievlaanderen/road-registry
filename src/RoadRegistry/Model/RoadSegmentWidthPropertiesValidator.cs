namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class RoadSegmentWidthPropertiesValidator : AbstractValidator<RequestedRoadSegmentWidthAttributes>
    {
        public RoadSegmentWidthPropertiesValidator()
        {
            RuleFor(c => c.FromPosition).GreaterThanOrEqualTo(0.0m);
            RuleFor(c => c.ToPosition).GreaterThan(_ => _.FromPosition);
            RuleFor(c => c.Width)
                .GreaterThanOrEqualTo(0)
                .When(_ => _.Width != RoadSegmentWidth.Unknown && _.Width != RoadSegmentWidth.NotApplicable, ApplyConditionTo.CurrentValidator);
        }
    }
}
