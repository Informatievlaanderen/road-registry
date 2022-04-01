namespace RoadRegistry.BackOffice.Validators.AddRoadSegment.Before
{
    using Core;
    using System;
    using System.Linq;
    using FluentValidation;
    using FluentValidation.Validators;
    using NetTopologySuite.Geometries;
    using Validators;

    internal class LinePointsValidator : PropertyValidator<AddRoadSegmentWithBeforeVerificationContext, LineString>
    {
        public override bool IsValid(ValidationContext<AddRoadSegmentWithBeforeVerificationContext> context, LineString value)
        {
            var line = context.InstanceToValidate.AddRoadSegment.Geometry.Geometries
                .OfType<LineString>()
                .Single();

            if (value.NumPoints > 0)
            {
                var previousPointMeasure = 0.0;
                for (var index = 0; index < value.CoordinateSequence.Count; index++)
                {
                    var measure = value.CoordinateSequence.GetOrdinate(index, Ordinate.M);
                    var x = value.CoordinateSequence.GetX(index);
                    var y = value.CoordinateSequence.GetY(index);
                    if (index == 0 && Math.Abs(measure) > context.InstanceToValidate.BeforeVerificationContext.Tolerances.MeasurementTolerance)
                    {
                        throw new RoadSegmentStartPointMeasureValueNotEqualToZero(x, y, measure).ToValidationException();
                    }

                    if (index == value.CoordinateSequence.Count - 1 && Math.Abs(measure - value.Length) > context.InstanceToValidate.BeforeVerificationContext.Tolerances.MeasurementTolerance)
                    {
                        throw new RoadSegmentEndPointMeasureValueNotEqualToLength(x, y, measure, line.Length).ToValidationException();
                    }

                    if (measure < 0.0 || measure > value.Length)
                    {
                        throw new RoadSegmentPointMeasureValueOutOfRange(x, y, measure, 0.0, line.Length).ToValidationException();
                    }

                    if (index != 0 && Math.Sign(measure - previousPointMeasure) <= 0)
                    {
                        throw new RoadSegmentPointMeasureValueDoesNotIncrease(x, y, measure, previousPointMeasure).ToValidationException();
                    }

                    previousPointMeasure = measure;
                }
            }

            return true;
        }

        public override string Name => nameof(LinePointsValidator);
    }
}
