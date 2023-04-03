namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using Abstractions.RoadSegments;
using Editor.Schema;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.ProblemCodes;
using Extensions;

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
            .WithProblemCode(ProblemCode.Common.JsonInvalid)
            .IsEnumName(typeof(ChangeRoadSegmentAttribute), false)
            .WithProblemCode(ProblemCode.RoadSegment.ChangeAttributesAttributeNotValid);

        RuleFor(x => x.Attribuutwaarde)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.Common.JsonInvalid);

        RuleFor(x => x.Wegsegmenten)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.Common.JsonInvalid)
            .MustAsync(BeExistingNonRemovedRoadSegment)
            .WithProblemCode(ProblemCode.RoadSegments.NotFound, wegsegmenten => string.Join(", ", FindNonExistingOrRemovedRoadSegmentIds(wegsegmenten)));

        When(x => IsEnum(x.Attribuut, ChangeRoadSegmentAttribute.Wegbeheerder), () =>
        {
            RuleFor(x => x.Attribuutwaarde)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.IsRequired)
                .MustAsync(BeKnownOrganization)
                .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.NotValid);
        });

        When(x => IsEnum(x.Attribuut, ChangeRoadSegmentAttribute.WegsegmentStatus), () =>
        {
            RuleFor(x => x.Attribuutwaarde)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.Status.IsRequired)
                .Must(value => RoadSegmentStatus.CanParseUsingDutchName(value) && RoadSegmentStatus.ParseUsingDutchName(value) != RoadSegmentStatus.Unknown)
                .WithProblemCode(ProblemCode.RoadSegment.Status.NotValid);
        });

        When(x => IsEnum(x.Attribuut, ChangeRoadSegmentAttribute.MorfologischeWegklasse), () =>
        {
            RuleFor(x => x.Attribuutwaarde)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.Morphology.IsRequired)
                .Must(value => RoadSegmentMorphology.CanParseUsingDutchName(value) && RoadSegmentMorphology.ParseUsingDutchName(value) != RoadSegmentMorphology.Unknown)
                .WithProblemCode(ProblemCode.RoadSegment.Morphology.NotValid);
        });

        When(x => IsEnum(x.Attribuut, ChangeRoadSegmentAttribute.Toegangsbeperking), () =>
        {
            RuleFor(x => x.Attribuutwaarde)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.IsRequired)
                .Must(RoadSegmentAccessRestriction.CanParseUsingDutchName)
                .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.NotValid);
        });

        When(x => IsEnum(x.Attribuut, ChangeRoadSegmentAttribute.Wegcategorie), () =>
        {
            RuleFor(x => x.Attribuutwaarde)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.Category.IsRequired)
                .Must(RoadSegmentCategory.CanParseUsingDutchName)
                .WithProblemCode(ProblemCode.RoadSegment.Category.NotValid);
        });
    }

    private Task<bool> BeKnownOrganization(string code, CancellationToken cancellationToken) => _editorContext.Organizations.AnyAsync(x => x.Code == code, cancellationToken);

    private Task<bool> BeExistingNonRemovedRoadSegment(int[] ids, CancellationToken cancellationToken) => Task.FromResult(!FindNonExistingOrRemovedRoadSegmentIds(ids.AsEnumerable()).Any());

    private IEnumerable<int> FindExistingAndNonRemovedRoadSegmentIds(IEnumerable<int> ids) => _editorContext.RoadSegments.Select(s => s.Id).Where(w => ids.Contains(w));

    private IEnumerable<int> FindNonExistingOrRemovedRoadSegmentIds(IEnumerable<int> ids) => ids.Except(FindExistingAndNonRemovedRoadSegmentIds(ids));

    private static bool IsEnum(string value, ChangeRoadSegmentAttribute enumMatch)
    {
        return Enum.TryParse(value, true, out ChangeRoadSegmentAttribute enumValue) && enumValue == enumMatch;
    }
}
