namespace RoadRegistry.BackOffice.Validators.AddRoadSegment.After
{
    using System.Linq;
    using FluentValidation;
    using NetTopologySuite.Geometries;

    internal class AddRoadSegmentWithAfterVerificationContextValidator : AbstractValidator<AddRoadSegmentWithAfterVerificationContext>
    {
        public AddRoadSegmentWithAfterVerificationContextValidator()
        {
            RuleFor(x => x.AfterVerificationContext.AfterView.Segments)
                .SetValidator(new GeometryTakenValidator());

            RuleForEach(x => x.AddRoadSegment.Geometry.Geometries.OfType<LineString>())
                .SetValidator(new LinesValidator());

            RuleFor(x => x.AddRoadSegment)
                .SetValidator(new IntersectingRoadSegmentsHaveNoGradeSeparatedJunctionsValidator());
        }
    }
}
