namespace RoadRegistry.BackOffice.Validators.AddRoadSegment.After
{
    using System.Collections.Generic;
    using System.Linq;
    using BackOffice;
    using Core;
    using FluentValidation;
    using FluentValidation.Validators;
    using Validators;

    internal class GeometryTakenValidator : PropertyValidator<AddRoadSegmentWithAfterVerificationContext, IReadOnlyDictionary<RoadSegmentId, RoadSegment>>
    {
        public override bool IsValid(ValidationContext<AddRoadSegmentWithAfterVerificationContext> context, IReadOnlyDictionary<RoadSegmentId, RoadSegment> value)
        {
            var byOtherSegment = value.Values.FirstOrDefault(segment =>
                segment.Id != context.InstanceToValidate.AddRoadSegment.Id &&
                segment.Geometry.EqualsWithinTolerance(context.InstanceToValidate.AddRoadSegment.Geometry, context.InstanceToValidate.AfterVerificationContext.Tolerances.GeometryTolerance));
            if (byOtherSegment != null)
            {
                throw new RoadSegmentGeometryTaken(
                    context.InstanceToValidate.AfterVerificationContext.Translator.TranslateToTemporaryOrId(byOtherSegment.Id)
                ).ToValidationException();
            }

            return true;
        }

        public override string Name => nameof(GeometryTakenValidator);
    }
}
