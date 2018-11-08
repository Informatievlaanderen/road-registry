namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class RoadSegmentSurfaceAttributesValidator : AbstractValidator<RequestedRoadSegmentSurfaceAttributes>
    {
        public RoadSegmentSurfaceAttributesValidator()
        {
            RuleFor(c => c.FromPosition).GreaterThanOrEqualTo(0.0m);
            RuleFor(c => c.ToPosition).GreaterThan(_ => _.FromPosition);
            RuleFor(c => c.Type).IsInEnum();
        }
    }
}
