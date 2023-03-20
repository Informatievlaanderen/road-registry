namespace RoadRegistry.BackOffice.Core;

using FluentValidation;
using FluentValidation.Results;

public abstract class ModifyRoadSegmentValidatorBase : AbstractValidator<Messages.ModifyRoadSegment>
{
    protected ModifyRoadSegmentValidatorBase()
    {
        RuleFor(c => c.Id).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Geometry).NotNull().SetValidator(new RoadSegmentGeometryValidator());
        RuleFor(c => c.MaintenanceAuthority).NotEmpty();
        RuleFor(c => c.GeometryDrawMethod)
            .NotEmpty()
            .Must(RoadSegmentGeometryDrawMethod.CanParse)
            .When(c => c.GeometryDrawMethod != null, ApplyConditionTo.CurrentValidator)
            .WithMessage("The 'GeometryDrawMethod' is not a RoadSegmentGeometryDrawMethod.");
        RuleFor(c => c.Morphology)
            .NotEmpty()
            .Must(RoadSegmentMorphology.CanParse)
            .When(c => c.Morphology != null, ApplyConditionTo.CurrentValidator)
            .WithMessage("The 'Morphology' is not a RoadSegmentMorphology.");
        RuleFor(c => c.Status)
            .NotEmpty()
            .Must(RoadSegmentStatus.CanParse)
            .When(c => c.Status != null, ApplyConditionTo.CurrentValidator)
            .WithMessage("The 'Status' is not a RoadSegmentStatus.");
        RuleFor(c => c.Category)
            .NotEmpty()
            .Must(RoadSegmentCategory.CanParse)
            .When(c => c.Category != null, ApplyConditionTo.CurrentValidator)
            .WithMessage("The 'Category' is not a RoadSegmentCategory.");
        RuleFor(c => c.AccessRestriction)
            .NotEmpty()
            .Must(RoadSegmentAccessRestriction.CanParse)
            .When(c => c.AccessRestriction != null, ApplyConditionTo.CurrentValidator)
            .WithMessage("The 'AccessRestriction' is not a RoadSegmentAccessRestriction.");

        RuleFor(c => c.Lanes).NotNull();
        RuleFor(c => c.Widths).NotNull();
        RuleFor(c => c.Surfaces).NotNull();
    }
}

public class ModifyRoadSegmentValidator : ModifyRoadSegmentValidatorBase
{
    protected override bool PreValidate(ValidationContext<Messages.ModifyRoadSegment> context, ValidationResult result)
    {
        if (context.InstanceToValidate.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined.ToString())
        {
            var validator = new ModifyRoadSegmentOutlineValidator();
            var outlineValidationResult = validator.Validate(context.InstanceToValidate);
            foreach (var failure in outlineValidationResult.Errors)
            {
                context.AddFailure(failure);
            }

            return false;
        }

        return true;
    }

    public ModifyRoadSegmentValidator()
    {
        RuleFor(c => c.StartNodeId)
            .GreaterThanOrEqualTo(0);

        RuleFor(c => c.EndNodeId)
            .GreaterThanOrEqualTo(0)
            .NotEqual(c => c.StartNodeId);

        RuleForEach(c => c.Lanes).NotNull().SetValidator(new RequestedRoadSegmentLaneAttributeValidator());
        RuleForEach(c => c.Widths).NotNull().SetValidator(new RequestedRoadSegmentWidthAttributeValidator());
        RuleForEach(c => c.Surfaces).NotNull().SetValidator(new RequestedRoadSegmentSurfaceAttributeValidator());
    }

    private sealed class ModifyRoadSegmentOutlineValidator : ModifyRoadSegmentValidatorBase
    {
        public ModifyRoadSegmentOutlineValidator()
        {
            RuleFor(c => c.StartNodeId)
                .Must(value => value.IsValidStartRoadNodeIdForRoadSegmentOutline());

            RuleFor(c => c.EndNodeId)
                .Must(value => value.IsValidEndRoadNodeIdForRoadSegmentOutline());

            RuleFor(c => c.Status)
                .NotEmpty()
                .Must(value => RoadSegmentStatus.CanParse(value) && RoadSegmentStatus.Parse(value).IsValidForRoadSegmentOutline())
                .WithErrorCode("InvalidStatus")
                .WithMessage("The 'Status' is not a valid RoadSegmentStatus.");

            RuleFor(c => c.Morphology)
                .NotEmpty()
                .Must(value => RoadSegmentMorphology.CanParse(value) && RoadSegmentMorphology.Parse(value).IsValidForRoadSegmentOutline())
                .When(c => c.Morphology != null, ApplyConditionTo.CurrentValidator)
                .WithMessage("The 'Morphology' is not a valid RoadSegmentMorphology.");

            RuleForEach(c => c.Lanes).NotNull().SetValidator(new RequestedRoadSegmentOutlineLaneAttributeValidator());
            RuleForEach(c => c.Widths).NotNull().SetValidator(new RequestedRoadSegmentOutlineWidthAttributeValidator());
            RuleForEach(c => c.Surfaces).NotNull().SetValidator(new RequestedRoadSegmentOutlineSurfaceAttributeValidator());
        }
    }
}
