namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class RoadSegmentHardeningPropertiesValidator : AbstractValidator<RequestedRoadSegmentHardeningProperties>
    {
        public RoadSegmentHardeningPropertiesValidator()
        {
            RuleFor(c => c.FromPosition).GreaterThanOrEqualTo(0.0);
            RuleFor(c => c.ToPosition).GreaterThan(_ => _.FromPosition);
            RuleFor(c => c.Type).IsInEnum();
        }
    }
}