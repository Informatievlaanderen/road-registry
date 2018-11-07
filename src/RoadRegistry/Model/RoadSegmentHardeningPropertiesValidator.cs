namespace RoadRegistry.Model
{
    using FluentValidation;
    using RoadRegistry.Commands;

    public class RoadSegmentHardeningPropertiesValidator : AbstractValidator<Shared.RoadSegmentHardeningProperties>
    {
        public RoadSegmentHardeningPropertiesValidator()
        {
            RuleFor(c => c.FromPosition).GreaterThanOrEqualTo(0.0);
            RuleFor(c => c.ToPosition).GreaterThan(_ => _.FromPosition);
            RuleFor(c => c.Type).IsInEnum();
        }
    }
}