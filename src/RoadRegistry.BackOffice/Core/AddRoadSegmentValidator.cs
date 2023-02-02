namespace RoadRegistry.BackOffice.Core;

using FluentValidation;
using FluentValidation.Results;

public abstract class AddRoadSegmentValidatorBase : AbstractValidator<Messages.AddRoadSegment>
{
    protected AddRoadSegmentValidatorBase()
    {
        RuleFor(c => c.TemporaryId)
            .GreaterThanOrEqualTo(0);

        RuleFor(c => c.Geometry)
            .NotNull()
            .SetValidator(new RoadSegmentGeometryValidator());

        RuleFor(c => c.GeometryDrawMethod)
            .NotEmpty()
            .Must(RoadSegmentGeometryDrawMethod.CanParse)
            .When(c => c.GeometryDrawMethod != null, ApplyConditionTo.CurrentValidator)
            .WithMessage("The 'GeometryDrawMethod' is not a RoadSegmentGeometryDrawMethod.");

        RuleFor(c => c.MaintenanceAuthority)
            .NotEmpty();

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

        RuleFor(c => c.AccessRestriction)
            .NotEmpty()
            .Must(RoadSegmentAccessRestriction.CanParse)
            .When(c => c.AccessRestriction != null, ApplyConditionTo.CurrentValidator)
            .WithMessage("The 'AccessRestriction' is not a RoadSegmentAccessRestriction.");

        RuleFor(c => c.Category)
            .NotEmpty()
            .Must(RoadSegmentCategory.CanParse)
            .When(c => c.Category != null, ApplyConditionTo.CurrentValidator)
            .WithMessage("The 'Category' is not a RoadSegmentCategory.");

        RuleFor(c => c.Lanes).NotNull();
        RuleFor(c => c.Widths).NotNull();
        RuleFor(c => c.Surfaces).NotNull();
    }
}

public class AddRoadSegmentValidator : AddRoadSegmentValidatorBase
{
    protected override bool PreValidate(ValidationContext<Messages.AddRoadSegment> context, ValidationResult result)
    {
        if (context.InstanceToValidate.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined.ToString())
        {
            var validator = new AddRoadSegmentOutlineValidator();
            var outlineValidationResult = validator.Validate(context.InstanceToValidate);
            foreach (var failure in outlineValidationResult.Errors)
            {
                context.AddFailure(failure);
            }

            return false;
        }

        return true;
    }
    
    public AddRoadSegmentValidator()
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

    private class AddRoadSegmentOutlineValidator : AddRoadSegmentValidatorBase
    {
        public AddRoadSegmentOutlineValidator()
        {
            RuleFor(c => c.StartNodeId)
                .Equal(0);

            RuleFor(c => c.EndNodeId)
                .Equal(0);
            
            RuleFor(c => c.Status)
                .NotEmpty()
                .Must(value => RoadSegmentStatus.CanParse(value) && RoadSegmentStatus.Parse(value) != RoadSegmentStatus.Unknown)
                .WithErrorCode("InvalidStatus")
                .WithMessage("The 'Status' is not a valid RoadSegmentStatus.");

            RuleFor(c => c.Morphology)
                .NotEmpty()
                .Must(value => RoadSegmentMorphology.CanParse(value) && RoadSegmentMorphology.Parse(value) != RoadSegmentMorphology.Unknown)
                .When(c => c.Morphology != null, ApplyConditionTo.CurrentValidator)
                .WithMessage("The 'Morphology' is not a valid RoadSegmentMorphology.");

            RuleForEach(c => c.Lanes).NotNull().SetValidator(new RequestedRoadSegmentOutlineLaneAttributeValidator());
            RuleForEach(c => c.Widths).NotNull().SetValidator(new RequestedRoadSegmentOutlineWidthAttributeValidator());
            RuleForEach(c => c.Surfaces).NotNull().SetValidator(new RequestedRoadSegmentOutlineSurfaceAttributeValidator());
        }
    }
}
