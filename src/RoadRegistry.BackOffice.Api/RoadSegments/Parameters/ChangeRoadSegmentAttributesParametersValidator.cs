namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice.Abstractions.Validation;
using RoadRegistry.Editor.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ChangeRoadSegmentAttributesParametersValidator : AbstractValidator<ChangeRoadSegmentAttributesParameters>
{
    private readonly EditorContext _editorContext;

    public ChangeRoadSegmentAttributesParametersValidator(EditorContext editorContext)
    {
        _editorContext = editorContext ?? throw new ArgumentNullException(nameof(editorContext));

        When(wegsegment => wegsegment.Wegsegmentstatus is not null, () =>
        {
            RuleFor(x => x.Wegsegmentstatus).Cascade(CascadeMode.Stop)
                .Must(value => RoadSegmentStatus.CanParseUsingDutchName(value) && RoadSegmentStatus.ParseUsingDutchName(value) != RoadSegmentStatus.Unknown)
                .WithErrorCode(ValidationErrors.RoadSegment.Status.NotParsed.Code)
                .WithMessage(x => ValidationErrors.RoadSegment.Status.NotParsed.Message(x.Wegsegmentstatus));
        });

        When(wegsegment => wegsegment.MorfologischeWegklasse is not null, () =>
        {
            RuleFor(x => x.MorfologischeWegklasse).Cascade(CascadeMode.Stop)
                .Must(value => RoadSegmentMorphology.CanParseUsingDutchName(value) && RoadSegmentMorphology.ParseUsingDutchName(value) != RoadSegmentMorphology.Unknown)
                .WithErrorCode(ValidationErrors.RoadSegment.Morphology.NotParsed.Code)
                .WithMessage(x => ValidationErrors.RoadSegment.Morphology.NotParsed.Message(x.MorfologischeWegklasse));
        });

        When(wegsegment => wegsegment.Toegangsbeperking is not null, () =>
        {
            RuleFor(x => x.Toegangsbeperking).Cascade(CascadeMode.Stop)
                .Must(RoadSegmentAccessRestriction.CanParseUsingDutchName)
                .WithErrorCode(ValidationErrors.RoadSegment.AccessRestriction.NotParsed.Code)
                .WithMessage(x => ValidationErrors.RoadSegment.AccessRestriction.NotParsed.Message(x.Toegangsbeperking));
        });

        When(wegsegment => wegsegment.Wegbeheerder is not null, () =>
        {
            RuleFor(x => x.Wegbeheerder)
                .Cascade(CascadeMode.Stop)
                .MustAsync(BeKnownOrganization)
                .WithErrorCode(ValidationErrors.RoadSegment.Organization.NotFound.Code)
                .WithMessage(x => ValidationErrors.RoadSegment.Organization.NotFound.Message(x.Wegbeheerder));
        });

        When(wegsegment => wegsegment.Wegcategorie is not null, () =>
        {
            RuleFor(x => x.Wegcategorie)
                .Cascade(CascadeMode.Stop)
                .Must(RoadSegmentCategory.CanParseUsingDutchName)
                .WithErrorCode(ValidationErrors.RoadSegment.Category.NotParsed.Code)
                .WithMessage(x => ValidationErrors.RoadSegment.Category.NotParsed.Message(x.Wegcategorie));
        });
    }

    protected override bool PreValidate(ValidationContext<ChangeRoadSegmentAttributesParameters> context, ValidationResult result)
    {
        var wegsegment = context.InstanceToValidate;

        if (wegsegment.Wegsegmentstatus is not null ||
            wegsegment.MorfologischeWegklasse is not null ||
            wegsegment.Toegangsbeperking is not null ||
            wegsegment.Wegbeheerder is not null ||
            wegsegment.Wegcategorie is not null)
        {
            return true;
        }

        context.AddFailure(new ValidationFailure
        {
            ErrorCode = ValidationErrors.RoadSegment.ChangeAttributesRequestNull.Code,
            ErrorMessage = ValidationErrors.RoadSegment.ChangeAttributesRequestNull.Message
        });

        return false;

    }

    private Task<bool> BeKnownOrganization(string code, CancellationToken cancellationToken)
    {
        return _editorContext.Organizations.AnyAsync(x => x.Code == code, cancellationToken);
    }
}
