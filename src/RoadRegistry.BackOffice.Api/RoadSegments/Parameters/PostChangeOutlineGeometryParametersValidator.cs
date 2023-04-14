namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using Core.ProblemCodes;
using Extensions;
using FluentValidation;

public class PostChangeOutlineGeometryParametersValidator : AbstractValidator<PostChangeOutlineGeometryParameters>
{
    public PostChangeOutlineGeometryParametersValidator()
    {
        RuleFor(x => x.MiddellijnGeometrie)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.Geometry.IsRequired)
            .Must(GeometryTranslator.GmlIsValidLineString)
            .WithProblemCode(ProblemCode.RoadSegment.Geometry.NotValid)
            .Must(gml => GeometryTranslator.ParseGmlLineString(gml).SRID == SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32())
            .WithProblemCode(ProblemCode.RoadSegment.Geometry.SridNotValid)
            .Must(gml => GeometryTranslator.ParseGmlLineString(gml).Length >= Distances.TooClose)
            .WithProblemCode(RoadSegmentGeometryLengthIsLessThanMinimum.ProblemCode, _ => new RoadSegmentGeometryLengthIsLessThanMinimum(Distances.TooClose));
    }
}
