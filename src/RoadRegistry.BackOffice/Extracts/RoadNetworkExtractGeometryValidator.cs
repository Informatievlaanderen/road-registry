namespace RoadRegistry.BackOffice.Extracts
{
    using FluentValidation;

    public class RoadNetworkExtractGeometryValidator : AbstractValidator<Messages.RoadNetworkExtractGeometry>
    {
        public RoadNetworkExtractGeometryValidator()
        {
            RuleFor(c => c.SpatialReferenceSystemIdentifier).GreaterThanOrEqualTo(0);
            RuleFor(c => c.MultiPolygon).NotNull();
            RuleForEach(c => c.MultiPolygon).NotNull().SetValidator(new PolygonValidator());
        }
    }
}