namespace RoadRegistry.BackOffice.Validators.AddRoadSegment.After
{
    using System.Linq;
    using Core;
    using FluentValidation;
    using FluentValidation.Validators;

    internal class IntersectingRoadSegmentsHaveNoGradeSeparatedJunctionsValidator : PropertyValidator<AddRoadSegmentWithAfterVerificationContext, AddRoadSegment>
    {
        public override bool IsValid(ValidationContext<AddRoadSegmentWithAfterVerificationContext> context, AddRoadSegment value)
        {
            var intersectingSegments = context.InstanceToValidate.AfterVerificationContext.AfterView.View.CreateScopedView(value.Geometry.EnvelopeInternal).FindIntersectingRoadSegments(value);
            var intersectingSegmentsWithoutJunction = intersectingSegments.Where(intersectingSegment =>
                !context.InstanceToValidate.AfterVerificationContext.AfterView.GradeSeparatedJunctions.Any(junction =>
                    (junction.Value.LowerSegment == value.Id && junction.Value.UpperSegment == intersectingSegment.Key) ||
                    (junction.Value.LowerSegment == intersectingSegment.Key && junction.Value.UpperSegment == value.Id)));

            var intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions = intersectingSegmentsWithoutJunction.Select(i =>
                new IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction(
                    context.InstanceToValidate.AfterVerificationContext.Translator.TranslateToTemporaryOrId(value.Id),
                    context.InstanceToValidate.AfterVerificationContext.Translator.TranslateToTemporaryOrId(i.Key))).ToList();

            if (intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions.Any())
            {
                throw intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions.ToValidationException();
            }

            return true;
        }

        public override string Name => nameof(IntersectingRoadSegmentsHaveNoGradeSeparatedJunctionsValidator);
    }
}
