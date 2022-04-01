namespace RoadRegistry.BackOffice.Validators.AddGradeSeparatedJunction.After
{
    using Core;
    using FluentValidation;
    using FluentValidation.Validators;

    internal class SegmentsValidator : PropertyValidator<AddGradeSeparatedJunctionWithAfterVerificationContext, AddGradeSeparatedJunction>
    {
        public override bool IsValid(ValidationContext<AddGradeSeparatedJunctionWithAfterVerificationContext> context, AddGradeSeparatedJunction value)
        {
            if (!context.InstanceToValidate.AfterVerificationContext.AfterView.View.Segments.TryGetValue(value.UpperSegmentId, out var upperSegment))
            {
                throw new UpperRoadSegmentMissing().ToValidationException();
            }

            if (!context.InstanceToValidate.AfterVerificationContext.AfterView.View.Segments.TryGetValue(value.LowerSegmentId, out var lowerSegment))
            {
                throw new LowerRoadSegmentMissing().ToValidationException();
            }

            if (upperSegment != null && lowerSegment != null)
            {
                if (!upperSegment.Geometry.Intersects(lowerSegment.Geometry))
                {
                    throw new UpperAndLowerRoadSegmentDoNotIntersect().ToValidationException();
                }
            }

            return true;
        }

        public override string Name => nameof(SegmentsValidator);
    }
}
