namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Core;
using Core.ProblemCodes;
using Editor.Schema;

public class PostRoadSegmentOutlineParametersValidator : AbstractValidator<PostRoadSegmentOutlineParameters>
{
    private readonly EditorContext _editorContext;

    public PostRoadSegmentOutlineParametersValidator(EditorContext editorContext)
    {
        _editorContext = editorContext ?? throw new ArgumentNullException(nameof(editorContext));

        RuleFor(x => x.MiddellijnGeometrie)
            .NotEmpty()
            .Must(GeometryTranslator.GmlIsValidLineString);

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
            .Must(width => RoadSegmentWidth.Accepts(width!.Value))
            .WithProblemCode(ProblemCode.RoadSegment.Width.NotValid);

        RuleFor(x => x.AantalRijstroken)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.Lane.IsRequired)
            .SetValidator(new RoadSegmentLaneParametersValidator());
    }

    private Task<bool> BeKnownOrganization(string code, CancellationToken cancellationToken)
    {
        return _editorContext.Organizations.AnyAsync(x => x.Code == code, cancellationToken);
    }
}
