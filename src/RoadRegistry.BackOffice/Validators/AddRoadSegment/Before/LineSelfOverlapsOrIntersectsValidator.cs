namespace RoadRegistry.BackOffice.Validators.AddRoadSegment.Before
{
    using Core;
    using FluentValidation;
    using FluentValidation.Validators;
    using NetTopologySuite.Geometries;
    using Validators;

    internal class LineSelfOverlapsOrIntersectsValidator : PropertyValidator<AddRoadSegmentWithBeforeVerificationContext, LineString>
    {
        public override bool IsValid(ValidationContext<AddRoadSegmentWithBeforeVerificationContext> context, LineString value)
        {
            if (value.SelfIntersects())
            {
                throw new RoadSegmentGeometrySelfIntersects().ToValidationException();
            }

            if (value.SelfOverlaps())
            {
                throw new RoadSegmentGeometrySelfOverlaps().ToValidationException();
            }

            return true;
        }

        public override string Name => nameof(LineSelfOverlapsOrIntersectsValidator);
    }
}
