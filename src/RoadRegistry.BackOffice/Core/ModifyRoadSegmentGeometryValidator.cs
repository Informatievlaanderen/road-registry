namespace RoadRegistry.BackOffice.Core;

using Extensions;
using FluentValidation;
using FluentValidation.Results;
using ProblemCodes;

public abstract class ModifyRoadSegmentGeometryValidatorBase : AbstractValidator<Messages.ModifyRoadSegmentGeometry>
{
    protected ModifyRoadSegmentGeometryValidatorBase()
    {
        RuleFor(c => c.Id).GreaterThanOrEqualTo(0);
        RuleFor(c => c.GeometryDrawMethod)
            .NotEmpty()
            .Must(RoadSegmentGeometryDrawMethod.CanParse)
            .When(c => c.GeometryDrawMethod != null, ApplyConditionTo.CurrentValidator)
            .WithProblemCode(ProblemCode.RoadSegment.GeometryDrawMethod.NotValid);
    }
}

public class ModifyRoadSegmentGeometryValidator : ModifyRoadSegmentGeometryValidatorBase
{
    protected override bool PreValidate(ValidationContext<Messages.ModifyRoadSegmentGeometry> context, ValidationResult result)
    {
        if (context.InstanceToValidate.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined.ToString())
        {
            var validator = new ModifyRoadSegmentGeometryOutlinedValidator();
            var outlineValidationResult = validator.Validate(context.InstanceToValidate);
            foreach (var failure in outlineValidationResult.Errors)
            {
                context.AddFailure(failure);
            }

            return false;
        }

        return true;
    }

    public ModifyRoadSegmentGeometryValidator()
    {
    }

    private sealed class ModifyRoadSegmentGeometryOutlinedValidator : ModifyRoadSegmentGeometryValidatorBase
    {
        public ModifyRoadSegmentGeometryOutlinedValidator()
        {
            RuleFor(x => x.Lanes)
                .NotEmpty()
                .WithProblemCode(ProblemCode.RoadSegment.Lanes.HasCountOfZero,
                    (segment, _) => new RoadSegmentLanesHasCountOfZero(segment.Id))
                .MaximumLength(1)
                .WithProblemCode(ProblemCode.RoadSegment.Lanes.CountGreaterThanOne,
                    (segment, _) => new RoadSegmentLanesCountGreaterThanOne(segment.Id));

            RuleFor(x => x.Surfaces)
                .NotEmpty()
                .WithProblemCode(ProblemCode.RoadSegment.Surfaces.HasCountOfZero,
                    (segment, _) => new RoadSegmentSurfacesHasCountOfZero(segment.Id))
                .MaximumLength(1)
                .WithProblemCode(ProblemCode.RoadSegment.Surfaces.CountGreaterThanOne,
                    (segment, _) => new RoadSegmentSurfacesCountGreaterThanOne(segment.Id));

            RuleFor(x => x.Widths)
                .NotEmpty()
                .WithProblemCode(ProblemCode.RoadSegment.Widths.HasCountOfZero,
                    (segment, _) => new RoadSegmentWidthsHasCountOfZero(segment.Id))
                .MaximumLength(1)
                .WithProblemCode(ProblemCode.RoadSegment.Widths.CountGreaterThanOne,
                    (segment, _) => new RoadSegmentWidthsCountGreaterThanOne(segment.Id));
        }
    }
}
