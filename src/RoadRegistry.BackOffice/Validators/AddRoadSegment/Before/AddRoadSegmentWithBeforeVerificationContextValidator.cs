namespace RoadRegistry.BackOffice.Validators.AddRoadSegment.Before
{
    using System.Linq;
    using FluentValidation;
    using NetTopologySuite.Geometries;

    internal class AddRoadSegmentWithBeforeVerificationContextValidator : AbstractValidator<AddRoadSegmentWithBeforeVerificationContext>
    {
        public AddRoadSegmentWithBeforeVerificationContextValidator()
        {
            // geometry length
            RuleFor(x => x.AddRoadSegment.Geometry)
                .SetValidator(new RoadSegmentGeometryLengthIsZeroValidator());

            // line self intersects
            RuleForEach(x => x.AddRoadSegment.Geometry.Geometries.OfType<LineString>())
                .SetValidator(new LineSelfOverlapsOrIntersectsValidator());

            // line points
            RuleForEach(x => x.AddRoadSegment.Geometry.Geometries.OfType<LineString>())
                .SetValidator(new LinePointsValidator());

            // lanes
            RuleFor(x => x.AddRoadSegment.Lanes)
                .SetValidator(new LanesValidator());

            // widths
            RuleFor(x => x.AddRoadSegment.Widths)
                .SetValidator(new WidthsValidator());

            // surfaces
            RuleFor(x => x.AddRoadSegment.Surfaces)
                .SetValidator(new SurfacesValidator());
        }
    }
}
