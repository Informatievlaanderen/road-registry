namespace RoadRegistry.BackOffice.Core;

using FluentValidation;
using Messages;

public class RoadSegmentGeometryValidator : AbstractValidator<RoadSegmentGeometry>
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
