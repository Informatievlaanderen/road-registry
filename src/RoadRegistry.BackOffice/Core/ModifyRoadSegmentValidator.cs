namespace RoadRegistry.BackOffice.Core;

using CommandHandling;
using FluentValidation;
using FluentValidation.Results;
using ValueObjects.ProblemCodes;
using ValueObjects.Problems;

public abstract class ModifyRoadSegmentValidatorBase : AbstractValidator<Messages.ModifyRoadSegment>
{
    protected ModifyRoadSegmentValidatorBase()
    {
        RuleFor(c => c.Id).GreaterThanOrEqualTo(0);

        RuleFor(c => c.GeometryDrawMethod)
            .NotEmpty()
            .Must(RoadSegmentGeometryDrawMethod.CanParse)
            .WithProblemCode(ProblemCode.RoadSegment.GeometryDrawMethod.NotValid)
            .When(c => c.GeometryDrawMethod != null, ApplyConditionTo.CurrentValidator);

        When(x => x.OriginalId is not null, () => { RuleFor(c => c.OriginalId).GreaterThan(0); });

        When(c => c.Geometry is not null, () => { RuleFor(c => c.Geometry).NotNull().SetValidator(new RoadSegmentGeometryValidator()); });

        When(c => c.MaintenanceAuthority is not null, () => { RuleFor(c => c.MaintenanceAuthority).NotEmpty(); });

        When(c => c.Morphology is not null, () =>
        {
            RuleFor(c => c.Morphology)
                .NotEmpty()
                .Must(RoadSegmentMorphology.CanParse)
                .WithProblemCode(ProblemCode.RoadSegment.Morphology.NotValid)
                .When(c => c.Morphology != null, ApplyConditionTo.CurrentValidator);
        });

        When(c => c.Status is not null, () =>
        {
            RuleFor(c => c.Status)
                .NotEmpty()
                .Must(RoadSegmentStatus.CanParse)
                .WithProblemCode(ProblemCode.RoadSegment.Status.NotValid)
                .When(c => c.Status != null, ApplyConditionTo.CurrentValidator);
        });

        When(c => c.Category is not null, () =>
        {
            RuleFor(c => c.Category)
                .NotEmpty()
                .Must(RoadSegmentCategory.CanParse)
                .WithProblemCode(ProblemCode.RoadSegment.Category.NotValid)
                .When(c => c.Category != null, ApplyConditionTo.CurrentValidator);
        });

        When(c => c.AccessRestriction is not null, () =>
        {
            RuleFor(c => c.AccessRestriction)
                .NotEmpty()
                .Must(RoadSegmentAccessRestriction.CanParse)
                .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.NotValid)
                .When(c => c.AccessRestriction != null, ApplyConditionTo.CurrentValidator);
        });

        When(c => c.Lanes is not null, () =>
        {
            RuleFor(c => c.Lanes)
                .NotEmpty()
                .WithProblemCode(ProblemCode.RoadSegment.Lanes.HasCountOfZero,
                    (segment, _) => new RoadSegmentLanesHasCountOfZero(segment.OriginalId ?? segment.Id));
        });

        When(c => c.Widths is not null, () =>
        {
            RuleFor(c => c.Widths)
                .NotEmpty()
                .WithProblemCode(ProblemCode.RoadSegment.Widths.HasCountOfZero,
                    (segment, _) => new RoadSegmentWidthsHasCountOfZero(segment.OriginalId ?? segment.Id));
        });

        When(c => c.Surfaces is not null, () =>
        {
            RuleFor(c => c.Surfaces)
                .NotEmpty()
                .WithProblemCode(ProblemCode.RoadSegment.Surfaces.HasCountOfZero,
                    (segment, _) => new RoadSegmentSurfacesHasCountOfZero(segment.OriginalId ?? segment.Id));
        });
    }
}

public class ModifyRoadSegmentValidator : ModifyRoadSegmentValidatorBase
{
    protected override bool PreValidate(ValidationContext<Messages.ModifyRoadSegment> context, ValidationResult result)
    {
        if (context.InstanceToValidate.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined.ToString())
        {
            var validator = new ModifyRoadSegmentOutlinedValidator();
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
        When(c => c.StartNodeId is not null, () =>
        {
            RuleFor(c => c.StartNodeId)
                .Must(x => RoadNodeId.Accepts(x!.Value));
        });

        When(c => c.EndNodeId is not null, () =>
        {
            RuleFor(c => c.EndNodeId)
                .Must(x => RoadNodeId.Accepts(x!.Value))
                .NotEqual(c => c.StartNodeId);
        });

        When(c => c.Lanes is not null, () =>
        {
            RuleForEach(c => c.Lanes)
                .NotEmpty()
                .WithProblemCode(ProblemCode.RoadSegment.Lanes.HasCountOfZero)
                .SetValidator(new RequestedRoadSegmentLaneAttributeValidator());
        });

        When(c => c.Widths is not null, () =>
        {
            RuleForEach(c => c.Widths)
                .NotEmpty()
                .WithProblemCode(ProblemCode.RoadSegment.Widths.HasCountOfZero)
                .SetValidator(new RequestedRoadSegmentWidthAttributeValidator());
        });

        When(c => c.Surfaces is not null, () =>
        {
            RuleForEach(c => c.Surfaces)
                .NotEmpty()
                .WithProblemCode(ProblemCode.RoadSegment.Surfaces.HasCountOfZero)
                .SetValidator(new RequestedRoadSegmentSurfaceAttributeValidator());
        });
    }

    private sealed class ModifyRoadSegmentOutlinedValidator : ModifyRoadSegmentValidatorBase
    {
        public ModifyRoadSegmentOutlinedValidator()
        {
            When(c => c.StartNodeId is not null, () =>
            {
                RuleFor(c => c.StartNodeId)
                    .Must(value => value is null || value.Value.IsValidStartRoadNodeIdForRoadSegmentOutline());
            });

            When(c => c.EndNodeId is not null, () =>
            {
                RuleFor(c => c.EndNodeId)
                    .Must(value => value is null || value.Value.IsValidEndRoadNodeIdForRoadSegmentOutline());
            });

            When(c => c.Status is not null, () =>
            {
                RuleFor(c => c.Status)
                    .NotEmpty()
                    .Must(value => RoadSegmentStatus.CanParse(value) && RoadSegmentStatus.Parse(value).IsValidForEdit())
                    .WithProblemCode(ProblemCode.RoadSegment.Status.NotValid);
            });

            When(c => c.Morphology is not null, () =>
            {
                RuleFor(c => c.Morphology)
                    .NotEmpty()
                    .Must(value => RoadSegmentMorphology.CanParse(value) && RoadSegmentMorphology.Parse(value).IsValidForEdit())
                    .WithProblemCode(ProblemCode.RoadSegment.Morphology.NotValid)
                    .When(c => c.Morphology != null, ApplyConditionTo.CurrentValidator);
            });

            When(c => c.Lanes is not null, () =>
            {
                RuleForEach(c => c.Lanes)
                    .NotEmpty()
                    .WithProblemCode(ProblemCode.RoadSegment.Lanes.HasCountOfZero)
                    .SetValidator(new RequestedRoadSegmentOutlinedLaneAttributeValidator());
            });

            When(c => c.Widths is not null, () =>
            {
                RuleForEach(c => c.Widths)
                    .NotEmpty()
                    .WithProblemCode(ProblemCode.RoadSegment.Widths.HasCountOfZero)
                    .SetValidator(new RequestedRoadSegmentOutlinedWidthAttributeValidator());
            });

            When(c => c.Surfaces is not null, () =>
            {
                RuleForEach(c => c.Surfaces)
                    .NotEmpty()
                    .WithProblemCode(ProblemCode.RoadSegment.Surfaces.HasCountOfZero)
                    .SetValidator(new RequestedRoadSegmentOutlinedSurfaceAttributeValidator());
            });
        }
    }
}
