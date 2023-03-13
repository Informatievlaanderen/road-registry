namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice.Abstractions.Validation;
using RoadRegistry.Editor.Schema;

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
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.Status.IsRequired.Code)
            .WithMessage(ValidationErrors.RoadSegmentOutline.Status.IsRequired.Message)
            .Must(value => RoadSegmentStatus.CanParseUsingDutchName(value) && RoadSegmentStatus.ParseUsingDutchName(value) != RoadSegmentStatus.Unknown)
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.Status.NotParsed.Code)
            .WithMessage(x => ValidationErrors.RoadSegmentOutline.Status.NotParsed.Message(x.Wegsegmentstatus));

        RuleFor(x => x.MorfologischeWegklasse)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.Morphology.IsRequired.Code)
            .WithMessage(ValidationErrors.RoadSegmentOutline.Morphology.IsRequired.Message)
            .Must(value => RoadSegmentMorphology.CanParseUsingDutchName(value) && RoadSegmentMorphology.ParseUsingDutchName(value) != RoadSegmentMorphology.Unknown)
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.Morphology.NotParsed.Code)
            .WithMessage(x => ValidationErrors.RoadSegmentOutline.Morphology.NotParsed.Message(x.MorfologischeWegklasse));

        RuleFor(x => x.Toegangsbeperking)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.AccessRestriction.IsRequired.Code)
            .WithMessage(ValidationErrors.RoadSegmentOutline.AccessRestriction.IsRequired.Message)
            .Must(RoadSegmentAccessRestriction.CanParseUsingDutchName)
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.AccessRestriction.NotParsed.Code)
            .WithMessage(x => ValidationErrors.RoadSegmentOutline.AccessRestriction.NotParsed.Message(x.Toegangsbeperking));

        RuleFor(x => x.Wegbeheerder)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.Organization.IsRequired.Code)
            .WithMessage(ValidationErrors.RoadSegmentOutline.Organization.IsRequired.Message)
            .MustAsync(BeKnownOrganization)
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.Organization.NotFound.Code)
            .WithMessage(x => ValidationErrors.RoadSegmentOutline.Organization.NotFound.Message(x.Wegbeheerder));

        var validSurfaceTypes = new[] { RoadSegmentSurfaceType.LooseSurface, RoadSegmentSurfaceType.SolidSurface };

        RuleFor(x => x.Wegverharding)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.SurfaceType.IsRequired.Code)
            .WithMessage(ValidationErrors.RoadSegmentOutline.SurfaceType.IsRequired.Message)
            .Must(x => RoadSegmentSurfaceType.CanParseUsingDutchName(x) && validSurfaceTypes.Contains(RoadSegmentSurfaceType.ParseUsingDutchName(x)))
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.SurfaceType.NotParsed.Code)
            .WithMessage(x => ValidationErrors.RoadSegmentOutline.SurfaceType.NotParsed.Message(x.Wegverharding));

        RuleFor(x => x.Wegbreedte)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0)
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.Width.GreaterThanZero.Code)
            .WithMessage(ValidationErrors.RoadSegmentOutline.Width.GreaterThanZero.Message)
            .Must(RoadSegmentWidth.Accepts)
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.Width.NotAccepted.Code)
            .WithMessage(x => ValidationErrors.RoadSegmentOutline.Width.NotAccepted.Message(x.Wegbreedte));

        RuleFor(x => x.AantalRijstroken)
            .NotNull()
            .WithErrorCode(ValidationErrors.RoadSegmentOutline.Lane.GreaterThanZero.Code)
            .WithMessage(ValidationErrors.RoadSegmentOutline.Lane.GreaterThanZero.Message)
            .SetValidator(new RoadSegmentLaneParametersValidator())
            .When(x => x.AantalRijstroken != null);
    }

    private Task<bool> BeKnownOrganization(string code, CancellationToken cancellationToken)
    {
        return _editorContext.Organizations.AnyAsync(x => x.Code == code, cancellationToken);
    }
}
