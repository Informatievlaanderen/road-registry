namespace RoadRegistry.BackOffice.Core;

using FluentValidation;
using Messages;

public class RoadNodeGeometryValidator : AbstractValidator<RoadNodeGeometry>
{
    public RoadNodeGeometryValidator()
    {
        RuleFor(c => c.SpatialReferenceSystemIdentifier).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Point).NotNull();
    }
}
