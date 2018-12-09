namespace RoadRegistry.Model
{
    using FluentValidation;

    public class RoadSegmentGeometryValidator : AbstractValidator<Messages.RoadSegmentGeometry>
    {
        public RoadSegmentGeometryValidator()
        {
            RuleFor(c => c.SpatialReferenceSystemIdentifier).GreaterThanOrEqualTo(0);
            RuleFor(c => c.MultiLineString)
                .NotNull()
                .MaximumLength(1)
                .When(c => !ReferenceEquals(c.MultiLineString, null), ApplyConditionTo.CurrentValidator);
            RuleForEach(c => c.MultiLineString).NotNull().SetValidator(new LineStringValidator());
        }
    }
}
