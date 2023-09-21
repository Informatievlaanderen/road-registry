namespace RoadRegistry.BackOffice.Core;

using Extensions;
using FluentValidation;
using FluentValidation.Results;
using ProblemCodes;

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
            .WithProblemCode(ProblemCode.RoadSegment.GeometryDrawMethod.NotValid)
            .When(c => c.GeometryDrawMethod != null, ApplyConditionTo.CurrentValidator);

        RuleFor(c => c.MaintenanceAuthority)
            .NotEmpty();

        RuleFor(c => c.Morphology)
            .NotEmpty()
            .Must(RoadSegmentMorphology.CanParse)
            .WithProblemCode(ProblemCode.RoadSegment.Morphology.NotValid)
            .When(c => c.Morphology != null, ApplyConditionTo.CurrentValidator);

        RuleFor(c => c.Status)
            .NotEmpty()
            .Must(RoadSegmentStatus.CanParse)
            .WithProblemCode(ProblemCode.RoadSegment.Status.NotValid)
            .When(c => c.Status != null, ApplyConditionTo.CurrentValidator);

        RuleFor(c => c.AccessRestriction)
            .NotEmpty()
            .Must(RoadSegmentAccessRestriction.CanParse)
            .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.NotValid)
            .When(c => c.AccessRestriction != null, ApplyConditionTo.CurrentValidator);

        RuleFor(c => c.Category)
            .NotEmpty()
            .Must(RoadSegmentCategory.CanParse)
            .WithProblemCode(ProblemCode.RoadSegment.Category.NotValid)
            .When(c => c.Category != null, ApplyConditionTo.CurrentValidator);

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
            var validator = new AddRoadSegmentOutlinedValidator();
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

    private sealed class AddRoadSegmentOutlinedValidator : AddRoadSegmentValidatorBase
    {
        public AddRoadSegmentOutlinedValidator()
        {
            RuleFor(c => c.StartNodeId)
                .Must(value => value.IsValidStartRoadNodeIdForRoadSegmentOutline());

            RuleFor(c => c.EndNodeId)
                .Must(value => value.IsValidEndRoadNodeIdForRoadSegmentOutline());

            RuleFor(c => c.Status)
                .NotEmpty()
                .Must(value => RoadSegmentStatus.CanParse(value) && RoadSegmentStatus.Parse(value).IsValidForEdit())
                .WithProblemCode(ProblemCode.RoadSegment.Status.NotValid);

            RuleFor(c => c.Morphology)
                .NotEmpty()
                .Must(value => RoadSegmentMorphology.CanParse(value) && RoadSegmentMorphology.Parse(value).IsValidForEdit())
                .When(c => c.Morphology != null, ApplyConditionTo.CurrentValidator)
                .WithProblemCode(ProblemCode.RoadSegment.Morphology.NotValid);

            RuleForEach(c => c.Lanes).NotNull().SetValidator(new RequestedRoadSegmentOutlinedLaneAttributeValidator());
            RuleForEach(c => c.Widths).NotNull().SetValidator(new RequestedRoadSegmentOutlinedWidthAttributeValidator());
            RuleForEach(c => c.Surfaces).NotNull().SetValidator(new RequestedRoadSegmentOutlinedSurfaceAttributeValidator());
        }
    }
}
