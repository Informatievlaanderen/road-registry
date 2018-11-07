namespace RoadRegistry.Model
{
    using FluentValidation;
    using RoadRegistry.Commands;

    public class RoadSegmentWidthPropertiesValidator : AbstractValidator<Shared.RoadSegmentWidthProperties>
    {
        public const int Unknown = -8;
        public const int NotApplicable = -9;

        public RoadSegmentWidthPropertiesValidator()
        {
            RuleFor(c => c.FromPosition).GreaterThanOrEqualTo(0.0);
            RuleFor(c => c.ToPosition).GreaterThan(_ => _.FromPosition);
            RuleFor(c => c.Width)
                .GreaterThanOrEqualTo(0)
                .When(_ => _.Width != Unknown && _.Width != NotApplicable, ApplyConditionTo.CurrentValidator);
        }
    }
}