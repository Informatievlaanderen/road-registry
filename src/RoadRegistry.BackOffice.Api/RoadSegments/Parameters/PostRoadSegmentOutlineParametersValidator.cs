namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using System;
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
            .WithErrorCode(ValidationErrors.RoadSegment.Status.IsRequired.Code)
            .WithMessage(ValidationErrors.RoadSegment.Status.IsRequired.Message)
            .Must(value => RoadSegmentStatus.CanParseUsingDutchName(value) && RoadSegmentStatus.ParseUsingDutchName(value) != RoadSegmentStatus.Unknown)
            .WithErrorCode(ValidationErrors.RoadSegment.Status.NotParsed.Code)
            .WithMessage(x => ValidationErrors.RoadSegment.Status.NotParsed.Message(x.Wegsegmentstatus));

        RuleFor(x => x.MorfologischeWegklasse)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(ValidationErrors.RoadSegment.Morphology.IsRequired.Code)
            .WithMessage(ValidationErrors.RoadSegment.Morphology.IsRequired.Message)
            .Must(value => RoadSegmentMorphology.CanParseUsingDutchName(value) && RoadSegmentMorphology.ParseUsingDutchName(value) != RoadSegmentMorphology.Unknown)
            .WithErrorCode(ValidationErrors.RoadSegment.Morphology.NotParsed.Code)
            .WithMessage(x => ValidationErrors.RoadSegment.Morphology.NotParsed.Message(x.MorfologischeWegklasse));

        RuleFor(x => x.Toegangsbeperking)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(ValidationErrors.RoadSegment.AccessRestriction.IsRequired.Code)
            .WithMessage(ValidationErrors.RoadSegment.AccessRestriction.IsRequired.Message)
            .Must(RoadSegmentAccessRestriction.CanParseUsingDutchName)
            .WithErrorCode(ValidationErrors.RoadSegment.AccessRestriction.NotParsed.Code)
            .WithMessage(x => ValidationErrors.RoadSegment.AccessRestriction.NotParsed.Message(x.Toegangsbeperking));

        RuleFor(x => x.Wegbeheerder)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(ValidationErrors.RoadSegment.Organization.IsRequired.Code)
            .WithMessage(ValidationErrors.RoadSegment.Organization.IsRequired.Message)
            .MustAsync(BeKnownOrganization)
            .WithErrorCode(ValidationErrors.RoadSegment.Organization.NotFound.Code)
            .WithMessage(x => ValidationErrors.RoadSegment.Organization.NotFound.Message(x.Wegbeheerder));

        RuleFor(x => x.Wegverharding)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(ValidationErrors.RoadSegment.SurfaceType.IsRequired.Code)
            .WithMessage(ValidationErrors.RoadSegment.SurfaceType.IsRequired.Message)
            .Must(RoadSegmentSurfaceType.CanParseUsingDutchName)
            .WithErrorCode(ValidationErrors.RoadSegment.SurfaceType.NotParsed.Code)
            .WithMessage(x => ValidationErrors.RoadSegment.SurfaceType.NotParsed.Message(x.Wegverharding));

        RuleFor(x => x.Wegbreedte)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0)
            .WithErrorCode(ValidationErrors.RoadSegment.Width.GreaterThanZero.Code)
            .WithMessage(ValidationErrors.RoadSegment.Width.GreaterThanZero.Message)
            .Must(RoadSegmentWidth.Accepts)
            .WithErrorCode(ValidationErrors.RoadSegment.Width.NotAccepted.Code)
            .WithMessage(x => ValidationErrors.RoadSegment.Width.NotAccepted.Message(x.Wegbreedte));

        RuleFor(x => x.AantalRijstroken)
            .NotNull()
            .WithErrorCode(ValidationErrors.RoadSegment.Lane.GreaterThanZero.Code)
            .WithMessage(ValidationErrors.RoadSegment.Lane.GreaterThanZero.Message)
            .SetValidator(new RoadSegmentLaneParametersValidator())
            .When(x => x.AantalRijstroken != null);
    }

    private Task<bool> BeKnownOrganization(string code, CancellationToken cancellationToken)
    {
        return _editorContext.Organizations.AnyAsync(x => x.Code == code, cancellationToken);
    }
}
