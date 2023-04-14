namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using Core.ProblemCodes;
using Editor.Schema;
using Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;

public class PostRoadSegmentOutlineParametersValidator : AbstractValidator<PostRoadSegmentOutlineParameters>
{
    private readonly EditorContext _editorContext;

    public PostRoadSegmentOutlineParametersValidator(EditorContext editorContext)
    {
        _editorContext = editorContext ?? throw new ArgumentNullException(nameof(editorContext));

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

        RuleFor(x => x.Wegsegmentstatus)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.Status.IsRequired)
            .Must(value => RoadSegmentStatus.CanParseUsingDutchName(value) && RoadSegmentStatus.ParseUsingDutchName(value).IsValidForRoadSegmentOutline())
            .WithProblemCode(ProblemCode.RoadSegment.Status.NotValid);

        RuleFor(x => x.MorfologischeWegklasse)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.Morphology.IsRequired)
            .Must(value => RoadSegmentMorphology.CanParseUsingDutchName(value) && RoadSegmentMorphology.ParseUsingDutchName(value).IsValidForRoadSegmentOutline())
            .WithProblemCode(ProblemCode.RoadSegment.Morphology.NotValid);

        RuleFor(x => x.Toegangsbeperking)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.IsRequired)
            .Must(RoadSegmentAccessRestriction.CanParseUsingDutchName)
            .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.NotValid);

        RuleFor(x => x.Wegbeheerder)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.IsRequired)
            .MustAsync(BeKnownOrganization)
            .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.NotValid);

        var validSurfaceTypes = new[] { RoadSegmentSurfaceType.LooseSurface, RoadSegmentSurfaceType.SolidSurface };

        RuleFor(x => x.Wegverharding)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.SurfaceType.IsRequired)
            .Must(x => RoadSegmentSurfaceType.CanParseUsingDutchName(x) && validSurfaceTypes.Contains(RoadSegmentSurfaceType.ParseUsingDutchName(x)))
            .WithProblemCode(ProblemCode.RoadSegment.SurfaceType.NotValid);

        RuleFor(x => x.Wegbreedte)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.Width.IsRequired)
            .LessThanOrEqualTo(RoadSegmentWidth.Maximum)
            .WithProblemCode(ProblemCode.RoadSegment.Width.LessThanOrEqualToMaximum)
            .Must(width => RoadSegmentWidth.Accepts(width!.Value))
            .WithProblemCode(ProblemCode.RoadSegment.Width.NotValid);

        RuleFor(x => x.AantalRijstroken)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.Lane.IsRequired)
            .SetValidator(new RoadSegmentLaneParametersValidator());
    }

    private Task<bool> BeKnownOrganization(string code, CancellationToken cancellationToken)
    {
        return _editorContext.Organizations.AnyAsync(x => x.Code == code, cancellationToken);
    }
}
