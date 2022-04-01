namespace RoadRegistry.BackOffice.Validators.AddRoadSegment.Before
{
    using System;
    using Core;
    using FluentValidation;
    using FluentValidation.Validators;
    using NetTopologySuite.Geometries;
    using Validators;

    internal class RoadSegmentGeometryLengthIsZeroValidator : PropertyValidator<AddRoadSegmentWithBeforeVerificationContext, MultiLineString>
    {
        public override bool IsValid(ValidationContext<AddRoadSegmentWithBeforeVerificationContext> context, MultiLineString value)
        {
            if (Math.Abs(value.Length) <= context.InstanceToValidate.BeforeVerificationContext.Tolerances.GeometryTolerance)
            {
                throw new RoadSegmentGeometryLengthIsZero().ToValidationException();
            }

            return true;
        }

        public override string Name => nameof(RoadSegmentGeometryLengthIsZeroValidator);
    }
}
