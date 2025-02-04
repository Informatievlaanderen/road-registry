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
using RoadRegistry.BackOffice.Handlers.Extensions;

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
            && context.InstanceToValidate.LinkerstraatnaamId is null
            && context.InstanceToValidate.RechterstraatnaamId is null
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

        When(x => x.EuropeseWegen is not null, () =>
        {
            RuleFor(x => x.EuropeseWegen)
                .Must(x => x.Length == x.Select(numberedRoad => numberedRoad?.EuNummer).Distinct().Count())
                .WithProblemCode(ProblemCode.RoadSegment.EuropeanRoads.NotUnique);

            RuleForEach(x => x.EuropeseWegen)
                .SetValidator(new ChangeAttributeEuropeanRoadValidator());
        });

        When(x => x.NationaleWegen is not null, () =>
        {
            RuleFor(x => x.NationaleWegen)
                .Must(x => x.Length == x.Select(numberedRoad => numberedRoad?.Ident2).Distinct().Count())
                .WithProblemCode(ProblemCode.RoadSegment.NationalRoads.NotUnique);

            RuleForEach(x => x.NationaleWegen)
                .SetValidator(new ChangeAttributeNationalRoadValidator());
        });

        When(x => x.GenummerdeWegen is not null, () =>
        {
            RuleFor(x => x.GenummerdeWegen)
                .Must(x => x.Length == x.Select(numberedRoad => numberedRoad?.Ident8).Distinct().Count())
                .WithProblemCode(ProblemCode.RoadSegment.NumberedRoads.NotUnique);

            RuleForEach(x => x.GenummerdeWegen)
                .SetValidator(new ChangeAttributeNumberedRoadValidator());
        });

        When(x => x.LinkerstraatnaamId is not null, () =>
        {
            RuleFor(x => x.LinkerstraatnaamId)
                .Cascade(CascadeMode.Stop)
                .MustBeValidStreetNameId(allowSystemValues: true)
                .WithProblemCode(ProblemCode.RoadSegment.StreetName.Left.NotValid);
        });

        When(x => x.RechterstraatnaamId is not null, () =>
        {
            RuleFor(x => x.RechterstraatnaamId)
                .Cascade(CascadeMode.Stop)
                .MustBeValidStreetNameId(allowSystemValues: true)
                .WithProblemCode(ProblemCode.RoadSegment.StreetName.Right.NotValid);
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

        var organization = await _organizationCache.FindByIdOrOvoCodeOrKboNumberAsync(new OrganizationId(code), cancellationToken);
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

public class ChangeAttributeEuropeanRoadValidator : AbstractValidator<ChangeAttributeEuropeanRoad>
{
    public ChangeAttributeEuropeanRoadValidator()
    {
        RuleFor(x => x.EuNummer)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.RoadSegment.EuropeanRoadNumber.IsRequired)
            .Must(EuropeanRoadNumber.CanParse)
            .WithProblemCode(ProblemCode.RoadSegment.EuropeanRoadNumber.NotValid);
    }
}

public class ChangeAttributeNationalRoadValidator : AbstractValidator<ChangeAttributeNationalRoad>
{
    public ChangeAttributeNationalRoadValidator()
    {
        RuleFor(x => x.Ident2)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.RoadSegment.NationalRoadNumber.IsRequired)
            .Must(NationalRoadNumber.CanParse)
            .WithProblemCode(ProblemCode.RoadSegment.NationalRoadNumber.NotValid);
    }
}

public class ChangeAttributeNumberedRoadValidator : AbstractValidator<ChangeAttributeNumberedRoad>
{
    public ChangeAttributeNumberedRoadValidator()
    {
        RuleFor(x => x.Ident8)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.RoadSegment.NumberedRoadNumber.IsRequired)
            .Must(NumberedRoadNumber.CanParse)
            .WithProblemCode(ProblemCode.RoadSegment.NumberedRoadNumber.NotValid);

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
            .Must(RoadSegmentNumberedRoadOrdinal.CanParseUsingDutchName)
            .WithProblemCode(ProblemCode.RoadSegment.NumberedRoadOrdinal.NotValid);
    }
}
