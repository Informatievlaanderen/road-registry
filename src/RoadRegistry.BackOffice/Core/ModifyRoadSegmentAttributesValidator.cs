namespace RoadRegistry.BackOffice.Core;

using System.Linq;
using Extensions;
using FluentValidation;
using FluentValidation.Results;
using ProblemCodes;

public abstract class ModifyRoadSegmentAttributesValidatorBase : AbstractValidator<Messages.ModifyRoadSegmentAttributes>
{
    protected ModifyRoadSegmentAttributesValidatorBase()
    {
        RuleFor(c => c.Id).GreaterThanOrEqualTo(0);
        RuleFor(c => c.GeometryDrawMethod)
            .NotEmpty()
            .Must(RoadSegmentGeometryDrawMethod.CanParse)
            .When(c => c.GeometryDrawMethod != null, ApplyConditionTo.CurrentValidator)
            .WithProblemCode(ProblemCode.RoadSegment.GeometryDrawMethod.NotValid);

        RuleFor(c => c.MaintenanceAuthority)
            .Must(OrganizationId.AcceptsValue)
            .When(c => c.MaintenanceAuthority != null, ApplyConditionTo.CurrentValidator)
            .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.NotValid);
        RuleFor(c => c.Morphology)
            .Must(RoadSegmentMorphology.CanParse)
            .When(c => c.Morphology != null, ApplyConditionTo.CurrentValidator)
            .WithProblemCode(ProblemCode.RoadSegment.Morphology.NotValid);
        RuleFor(c => c.Status)
            .Must(RoadSegmentStatus.CanParse)
            .When(c => c.Status != null, ApplyConditionTo.CurrentValidator)
            .WithProblemCode(ProblemCode.RoadSegment.Status.NotValid);
        RuleFor(c => c.Category)
            .Must(RoadSegmentCategory.CanParse)
            .When(c => c.Category != null, ApplyConditionTo.CurrentValidator)
            .WithProblemCode(ProblemCode.RoadSegment.Category.NotValid);
        RuleFor(c => c.AccessRestriction)
            .Must(RoadSegmentAccessRestriction.CanParse)
            .When(c => c.AccessRestriction != null, ApplyConditionTo.CurrentValidator)
            .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.NotValid);

        //TODO-rik add test
        When(x => x.EuropeanRoads is not null, () =>
        {
            RuleFor(x => x.EuropeanRoads)
                .Must(x => x.Length == x.Select(item => item.Number).Distinct().Count())
                .WithProblemCode(ProblemCode.RoadSegment.EuropeanRoad.IdentifierNotUnique);
            
            RuleForEach(x => x.EuropeanRoads)
                .SetValidator(new RoadSegmentEuropeanRoadAttributeValidator());
        });

        //TODO-rik add test
        When(x => x.NationalRoads is not null, () =>
        {
            RuleFor(x => x.NationalRoads)
                .Must(x => x.Length == x.Select(item => item.Number).Distinct().Count())
                .WithProblemCode(ProblemCode.RoadSegment.NationalRoad.IdentifierNotUnique);
            
            RuleForEach(x => x.NationalRoads)
                .SetValidator(new RoadSegmentNationalRoadAttributeValidator());
        });

        //TODO-rik add test
        When(x => x.NumberedRoads is not null, () =>
        {
            RuleFor(x => x.NumberedRoads)
                .Must(x => x.Length == x.Select(item => item.Number).Distinct().Count())
                .WithProblemCode(ProblemCode.RoadSegment.NumberedRoad.IdentifierNotUnique);

            RuleForEach(x => x.NumberedRoads)
                .SetValidator(new RoadSegmentNumberedRoadAttributeValidator());
        });
    }
}

public class ModifyRoadSegmentAttributesValidator : ModifyRoadSegmentAttributesValidatorBase
{
    protected override bool PreValidate(ValidationContext<Messages.ModifyRoadSegmentAttributes> context, ValidationResult result)
    {
        if (context.InstanceToValidate.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined.ToString())
        {
            var validator = new ModifyRoadSegmentAttributesOutlinedValidator();
            var outlineValidationResult = validator.Validate(context.InstanceToValidate);
            foreach (var failure in outlineValidationResult.Errors)
            {
                context.AddFailure(failure);
            }

            return false;
        }

        return true;
    }

    public ModifyRoadSegmentAttributesValidator()
    {
    }

    private sealed class ModifyRoadSegmentAttributesOutlinedValidator : ModifyRoadSegmentAttributesValidatorBase
    {
        public ModifyRoadSegmentAttributesOutlinedValidator()
        {
            RuleFor(c => c.Status)
                .Must(value => RoadSegmentStatus.CanParse(value) && RoadSegmentStatus.Parse(value).IsValidForEdit())
                .When(c => c.Status != null, ApplyConditionTo.CurrentValidator)
                .WithProblemCode(ProblemCode.RoadSegment.Status.NotValid);

            RuleFor(c => c.Morphology)
                .Must(value => RoadSegmentMorphology.CanParse(value) && RoadSegmentMorphology.Parse(value).IsValidForEdit())
                .When(c => c.Morphology != null, ApplyConditionTo.CurrentValidator)
                .WithProblemCode(ProblemCode.RoadSegment.Morphology.NotValid);
        }
    }
}
