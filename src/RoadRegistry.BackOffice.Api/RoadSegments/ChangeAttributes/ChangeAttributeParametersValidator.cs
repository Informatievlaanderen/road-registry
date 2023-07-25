namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeAttributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice.Core.ProblemCodes;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.Editor.Schema;

public class ChangeAttributeParametersValidator : AbstractValidator<ChangeAttributeParameters>
{
    private readonly EditorContext _editorContext;

    protected override bool PreValidate(ValidationContext<ChangeAttributeParameters> context, ValidationResult result)
    {
        if (context.InstanceToValidate is not null
            && context.InstanceToValidate.MorfologischeWegklasse is null
            && context.InstanceToValidate.Toegangsbeperking is null
            && context.InstanceToValidate.Wegbeheerder is null
            && context.InstanceToValidate.Wegcategorie is null
            && context.InstanceToValidate.Wegsegmentstatus is null
           )
        {
            context.AddFailure(new ValidationFailure
            {
                PropertyName = "attribuut",
                ErrorCode = ProblemCode.Common.JsonInvalid
            });

            return false;
        }

        return true;
    }

    public ChangeAttributeParametersValidator(EditorContext editorContext)
    {
        _editorContext = editorContext.ThrowIfNull();

        RuleFor(x => x.Wegsegmenten)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.Common.JsonInvalid)
            .Must(wegsegmenten => wegsegmenten.All(RoadSegmentId.Accepts))
            .WithProblemCode(ProblemCode.Common.JsonInvalid)
            .MustAsync(BeExistingNonRemovedRoadSegment)
            .WithProblemCode(ProblemCode.RoadSegments.NotFound, wegsegmenten => string.Join(", ", FindNonExistingOrRemovedRoadSegmentIds(wegsegmenten)));

        When(x => x.Wegbeheerder is not null, () =>
        {
            RuleFor(x => x.Wegbeheerder)
                .Cascade(CascadeMode.Stop)
                .MustAsync(BeKnownOrganization)
                .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.NotValid);
        });

        When(x => x.Wegsegmentstatus is not null, () =>
        {
            RuleFor(x => x.Wegsegmentstatus)
                .Cascade(CascadeMode.Stop)
                .Must(value => RoadSegmentStatus.CanParseUsingDutchName(value) && RoadSegmentStatus.ParseUsingDutchName(value).IsValidForEdit())
                .WithProblemCode(ProblemCode.RoadSegment.Status.NotValid);
        });

        When(x => x.MorfologischeWegklasse is not null, () =>
        {
            RuleFor(x => x.MorfologischeWegklasse)
                .Cascade(CascadeMode.Stop)
                .Must(value => RoadSegmentMorphology.CanParseUsingDutchName(value) && RoadSegmentMorphology.ParseUsingDutchName(value).IsValidForEdit())
                .WithProblemCode(ProblemCode.RoadSegment.Morphology.NotValid);
        });

        When(x => x.Toegangsbeperking is not null, () =>
        {
            RuleFor(x => x.Toegangsbeperking)
                .Cascade(CascadeMode.Stop)
                .Must(RoadSegmentAccessRestriction.CanParseUsingDutchName)
                .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.NotValid);
        });

        When(x => x.Wegcategorie is not null, () =>
        {
            RuleFor(x => x.Wegcategorie)
                .Cascade(CascadeMode.Stop)
                .Must(RoadSegmentCategory.CanParseUsingDutchName)
                .WithProblemCode(ProblemCode.RoadSegment.Category.NotValid);
        });
    }

    private Task<bool> BeExistingNonRemovedRoadSegment(int[] ids, CancellationToken cancellationToken)
    {
        return Task.FromResult(!FindNonExistingOrRemovedRoadSegmentIds(ids).Any());
    }

    private Task<bool> BeKnownOrganization(string code, CancellationToken cancellationToken)
    {
        return _editorContext.Organizations.AnyAsync(x => x.Code == code, cancellationToken);
    }

    private IEnumerable<int> FindExistingAndNonRemovedRoadSegmentIds(IEnumerable<int> ids)
    {
        return _editorContext.RoadSegments.Select(s => s.Id).Where(w => ids.Contains(w));
    }

    private IEnumerable<int> FindNonExistingOrRemovedRoadSegmentIds(ICollection<int> ids)
    {
        return ids.Except(FindExistingAndNonRemovedRoadSegmentIds(ids));
    }
}
