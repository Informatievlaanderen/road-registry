namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.RoadSegments;
using Abstractions.Validation;
using Amazon.DynamoDBv2.Model;
using Editor.Schema;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Geen wijzigingen op dynamisch gesegmenteerde attributen (wegverharding/wegbreedte/aantal rijstroken)
/// </summary>
public class ChangeAttributeParametersValidator : AbstractValidator<ChangeAttributeParameters>
{
    private readonly EditorContext _editorContext;

    public ChangeAttributeParametersValidator(EditorContext editorContext)
    {
        _editorContext = editorContext ?? throw new ArgumentNullException(nameof(editorContext));

        RuleFor(x => x.Attribuut)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode("JsonInvalid")
            .WithMessage("Json is not valid.")
            .IsEnumName(typeof(ChangeRoadSegmentAttributesEnum), false)
            .WithName("attribuut")
            .WithErrorCode("AttribuutNietGekend")
            .WithMessage(x => $"De waarde '{x.Attribuut}' komt niet overeen met een attribuut uit het Wegenregister dat via dit endpoint gewijzigd kan worden.");

        RuleFor(x => x.Attribuutwaarde)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode("JsonInvalid")
            .WithMessage("Json is not valid.");

        RuleFor(x => x.Wegsegmenten)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode("JsonInvalid")
            .WithMessage("Json is not valid.")
            .MustAsync(BeExistingNonRemovedRoadSegment)
            .WithErrorCode(ValidationErrors.RoadSegment.NotFound.Code)
            .WithMessage(x => ValidationErrors.RoadSegment.NotFound.FormattedMessage(FindNonExistingOrRemovedRoadSegmentIds(x.Wegsegmenten)));

        When(x => IsEnum(x.Attribuut, ChangeRoadSegmentAttributesEnum.Wegbeheerder), () =>
        {
            RuleFor(x => x.Attribuutwaarde)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithErrorCode(ValidationErrors.RoadSegment.Organization.IsRequired.Code)
                .WithMessage(ValidationErrors.RoadSegment.Organization.IsRequired.Message)
                .MustAsync(BeKnownOrganization)
                .WithErrorCode(ValidationErrors.RoadSegment.Organization.NotFound.Code)
                .WithMessage(x => ValidationErrors.RoadSegment.Organization.NotFound.Message(x.Attribuutwaarde));
        });

        When(x => IsEnum(x.Attribuut, ChangeRoadSegmentAttributesEnum.WegsegmentStatus), () =>
        {
            RuleFor(x => x.Attribuutwaarde)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithErrorCode(ValidationErrors.RoadSegment.Status.IsRequired.Code)
                .WithMessage(ValidationErrors.RoadSegment.Status.IsRequired.Message)
                .Must(value => RoadSegmentStatus.CanParseUsingDutchName(value) && RoadSegmentStatus.ParseUsingDutchName(value) != RoadSegmentStatus.Unknown)
                .WithErrorCode(ValidationErrors.RoadSegment.Status.NotParsed.Code)
                .WithMessage(x => ValidationErrors.RoadSegment.Status.NotParsed.Message(x.Attribuutwaarde));
        });

        When(x => IsEnum(x.Attribuut, ChangeRoadSegmentAttributesEnum.MorfologischeWegklasse), () =>
        {
            RuleFor(x => x.Attribuutwaarde)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithErrorCode(ValidationErrors.RoadSegment.Morphology.IsRequired.Code)
                .WithMessage(ValidationErrors.RoadSegment.Morphology.IsRequired.Message)
                .Must(value => RoadSegmentMorphology.CanParseUsingDutchName(value) && RoadSegmentMorphology.ParseUsingDutchName(value) != RoadSegmentMorphology.Unknown)
                .WithErrorCode(ValidationErrors.RoadSegment.Morphology.NotParsed.Code)
                .WithMessage(x => ValidationErrors.RoadSegment.Morphology.NotParsed.Message(x.Attribuutwaarde));
        });

        When(x => IsEnum(x.Attribuut, ChangeRoadSegmentAttributesEnum.Toegangsbeperking), () =>
        {
            RuleFor(x => x.Attribuutwaarde)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithErrorCode(ValidationErrors.RoadSegment.AccessRestriction.IsRequired.Code)
                .WithMessage(ValidationErrors.RoadSegment.AccessRestriction.IsRequired.Message)
                .Must(RoadSegmentAccessRestriction.CanParseUsingDutchName)
                .WithErrorCode(ValidationErrors.RoadSegment.AccessRestriction.NotParsed.Code)
                .WithMessage(x => ValidationErrors.RoadSegment.AccessRestriction.NotParsed.Message(x.Attribuutwaarde));
        });

        When(x => IsEnum(x.Attribuut, ChangeRoadSegmentAttributesEnum.Wegcategorie), () =>
        {
            RuleFor(x => x.Attribuutwaarde)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithErrorCode(ValidationErrors.RoadSegment.Category.IsRequired.Code)
                .WithMessage(ValidationErrors.RoadSegment.Category.IsRequired.Message)
                .Must(RoadSegmentCategory.CanParseUsingDutchName)
                .WithErrorCode(ValidationErrors.RoadSegment.Category.NotParsed.Code)
                .WithMessage(x => ValidationErrors.RoadSegment.Category.NotParsed.Message(x.Attribuutwaarde));

        });
    }

    private Task<bool> BeKnownOrganization(string code, CancellationToken cancellationToken) => _editorContext.Organizations.AnyAsync(x => x.Code == code, cancellationToken);

    private Task<bool> BeExistingNonRemovedRoadSegment(int[] ids, CancellationToken cancellationToken) => Task.FromResult(!FindNonExistingOrRemovedRoadSegmentIds(ids.AsEnumerable()).Any());

    private IEnumerable<int> FindExistingAndNonRemovedRoadSegmentIds(IEnumerable<int> ids) => _editorContext.RoadSegments.Select(s => s.Id).Where(w => ids.Contains(w));

    private IEnumerable<int> FindNonExistingOrRemovedRoadSegmentIds(IEnumerable<int> ids) => ids.Except(FindExistingAndNonRemovedRoadSegmentIds(ids));

    private static bool IsEnum(string value, ChangeRoadSegmentAttributesEnum enumMatch)
    {
        return Enum.TryParse(value, true, out ChangeRoadSegmentAttributesEnum enumValue) && enumValue == enumMatch;
    }
}
