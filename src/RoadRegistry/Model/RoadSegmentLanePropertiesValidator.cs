namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class RoadSegmentLanePropertiesValidator : AbstractValidator<RequestedRoadSegmentLaneProperties>
    {
        public const int Unknown = -8;
        public const int NotApplicable = -9;

        public RoadSegmentLanePropertiesValidator()
        {
            RuleFor(c => c.FromPosition).GreaterThanOrEqualTo(0.0);
            RuleFor(c => c.ToPosition).GreaterThan(_ => _.FromPosition);
            RuleFor(c => c.Count)
                .GreaterThanOrEqualTo(0)
                .When(_ => _.Count != Unknown && _.Count != NotApplicable, ApplyConditionTo.CurrentValidator);
            RuleFor(c => c.Direction).IsInEnum();
        }
    }
}