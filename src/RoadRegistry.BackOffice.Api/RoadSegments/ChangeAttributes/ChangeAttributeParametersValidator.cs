namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeAttributes;

using Core;
using Core.ProblemCodes;
using Editor.Schema;
using Extensions;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ChangeAttributeParametersValidator : AbstractValidator<ChangeAttributeParameters>
{
    private readonly IOrganizationCache _organizationCache;
    private readonly EditorContext _editorContext;

    protected override bool PreValidate(ValidationContext<ChangeAttributeParameters> context, ValidationResult result)
    {
        if (context.InstanceToValidate is not null
            && context.InstanceToValidate.MorfologischeWegklasse is null
            && context.InstanceToValidate.Toegangsbeperking is null
            && context.InstanceToValidate.Wegbeheerder is null
            && context.InstanceToValidate.Wegcategorie is null
            && context.InstanceToValidate.Wegsegmentstatus is null
            && context.InstanceToValidate.EuropeseWegen is null
            && context.InstanceToValidate.NationaleWegen is null
            && context.InstanceToValidate.GenummerdeWegen is null
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

    public ChangeAttributeParametersValidator(EditorContext editorContext, IOrganizationCache organizationCache)
    {
        _editorContext = editorContext.ThrowIfNull();
        _organizationCache = organizationCache.ThrowIfNull();

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
                .Must(OrganizationId.AcceptsValue)
                .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.NotValid)
                .MustAsync(BeKnownOrganization)
                .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.NotKnown, value => new MaintenanceAuthorityNotKnown(new OrganizationId(value)));
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

        //TODO-rik add test
        When(x => x.EuropeseWegen is not null, () =>
        {
            RuleFor(x => x.EuropeseWegen)
                .Must(x => x.Length == x.Distinct().Count())
                .WithProblemCode(ProblemCode.RoadSegment.EuropeanRoad.IdentifierNotUnique);

            RuleForEach(x => x.EuropeseWegen)
                .Cascade(CascadeMode.Stop)
                .Must(EuropeanRoadNumber.CanParse)
                .WithProblemCode(ProblemCode.RoadSegment.EuropeanRoad.NotValid);
        });

        //TODO-rik add test
        When(x => x.NationaleWegen is not null, () =>
        {
            RuleFor(x => x.NationaleWegen)
                .Must(x => x.Length == x.Distinct().Count())
                .WithProblemCode(ProblemCode.RoadSegment.NationalRoad.IdentifierNotUnique);

            RuleForEach(x => x.NationaleWegen)
                .Cascade(CascadeMode.Stop)
                .Must(NationalRoadNumber.CanParse)
                .WithProblemCode(ProblemCode.RoadSegment.NationalRoad.NotValid);
        });

        //TODO-rik add test
        When(x => x.GenummerdeWegen is not null, () =>
        {
            RuleFor(x => x.GenummerdeWegen)
                .Must(x => x.Length == x.Select(numberedRoad => numberedRoad.Ident8).Distinct().Count())
                .WithProblemCode(ProblemCode.RoadSegment.NumberedRoad.IdentifierNotUnique);

            RuleForEach(x => x.GenummerdeWegen)
                .SetValidator(new ChangeAttributeNumberedRoadValidator());
        });
    }

    private Task<bool> BeExistingNonRemovedRoadSegment(int[] ids, CancellationToken cancellationToken)
    {
        return Task.FromResult(!FindNonExistingOrRemovedRoadSegmentIds(ids).Any());
    }

    private async Task<bool> BeKnownOrganization(string code, CancellationToken cancellationToken)
    {
        if (!OrganizationId.AcceptsValue(code))
        {
            return false;
        }

        var organization = await _organizationCache.FindByIdOrOvoCodeAsync(new OrganizationId(code), cancellationToken);
        return organization is not null;
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

public class ChangeAttributeNumberedRoadValidator : AbstractValidator<ChangeAttributeNumberedRoad>
{
    public ChangeAttributeNumberedRoadValidator()
    {
        RuleFor(x => x.Ident8)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.RoadSegment.NumberedRoad.IsRequired)
            .Must(NumberedRoadNumber.CanParse)
            .WithProblemCode(ProblemCode.RoadSegment.NumberedRoad.NotValid);

        RuleFor(x => x.Richting)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.RoadSegment.NumberedRoadDirection.IsRequired)
            .Must(RoadSegmentNumberedRoadDirection.CanParseUsingDutchName)
            .WithProblemCode(ProblemCode.RoadSegment.NumberedRoadDirection.NotValid);

        RuleFor(x => x.Volgnummer)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.RoadSegment.NumberedRoadOrdinal.IsRequired)
            .Must(x => int.TryParse(x, out var intValue) && RoadSegmentNumberedRoadOrdinal.Accepts(intValue))
            .WithProblemCode(ProblemCode.RoadSegment.NumberedRoadOrdinal.NotValid);
    }
}
