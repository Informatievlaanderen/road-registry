namespace RoadRegistry.BackOffice.Extracts;

using FluentValidation;
using Messages;

public class RoadNetworkExtractGeometryValidator : AbstractValidator<RoadNetworkExtractGeometry>
{
    public RoadNetworkExtractGeometryValidator()
    {
        RuleFor(c => c.SpatialReferenceSystemIdentifier).GreaterThanOrEqualTo(0);
        RuleFor(c => c.MultiPolygon).NotNull().When(c => c.Polygon == null);
        RuleFor(c => c.Polygon).NotNull().When(c => c.MultiPolygon == null).SetValidator(new PolygonValidator());
        RuleForEach(c => c.MultiPolygon).NotNull().When(c => c.Polygon == null).SetValidator(new PolygonValidator());
    }
}
