namespace RoadRegistry.BackOffice.Validators.AddRoadSegment.Before
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BackOffice;
    using Core;
    using FluentValidation;
    using FluentValidation.Validators;
    using NetTopologySuite.Geometries;
    using Validators;

    internal class LanesValidator : PropertyValidator<AddRoadSegmentWithBeforeVerificationContext, IReadOnlyList<RoadSegmentLaneAttribute>>
    {
        public override bool IsValid(ValidationContext<AddRoadSegmentWithBeforeVerificationContext> context, IReadOnlyList<RoadSegmentLaneAttribute> value)
        {
            var line = context.InstanceToValidate.AddRoadSegment.Geometry.Geometries
                .OfType<LineString>()
                .Single();

            RoadSegmentLaneAttribute previousLane = null;
            foreach (var lane in value)
            {
                if (previousLane == null)
                {
                    if (lane.From != RoadSegmentPosition.Zero)
                    {
                        throw new RoadSegmentLaneAttributeFromPositionNotEqualToZero(
                            lane.TemporaryId,
                            lane.From).ToValidationException();
                    }
                }
                else
                {
                    if (lane.From != previousLane.To)
                    {
                        throw new RoadSegmentLaneAttributesNotAdjacent(
                            previousLane.TemporaryId,
                            previousLane.To,
                            lane.TemporaryId,
                            lane.From).ToValidationException();
                    }

                    if (lane.From == lane.To)
                    {
                        throw new RoadSegmentLaneAttributeHasLengthOfZero(
                            lane.TemporaryId,
                            lane.From,
                            lane.To).ToValidationException();
                    }
                }

                previousLane = lane;
            }

            if (previousLane != null)
            {
                if (Math.Abs(previousLane.To.ToDouble() - line.Length) > context.InstanceToValidate.BeforeVerificationContext.Tolerances.DynamicRoadSegmentAttributePositionTolerance)
                {
                    throw new RoadSegmentLaneAttributeToPositionNotEqualToLength(
                        previousLane.TemporaryId,
                        previousLane.To,
                        line.Length).ToValidationException();
                }
            }

            return true;
        }

        public override string Name => nameof(LanesValidator);
    }
}
