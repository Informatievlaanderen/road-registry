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

    internal class WidthsValidator : PropertyValidator<AddRoadSegmentWithBeforeVerificationContext, IReadOnlyList<RoadSegmentWidthAttribute>>
    {
        public override bool IsValid(ValidationContext<AddRoadSegmentWithBeforeVerificationContext> context, IReadOnlyList<RoadSegmentWidthAttribute> value)
        {
            var line = context.InstanceToValidate.AddRoadSegment.Geometry.Geometries
                .OfType<LineString>()
                .Single();

            RoadSegmentWidthAttribute previousWidth = null;
            foreach (var width in value)
            {
                if (previousWidth == null)
                {
                    if (width.From != RoadSegmentPosition.Zero)
                    {
                        throw new RoadSegmentWidthAttributeFromPositionNotEqualToZero(
                                width.TemporaryId,
                                width.From).ToValidationException();
                    }
                }
                else
                {
                    if (width.From != previousWidth.To)
                    {
                        throw new RoadSegmentWidthAttributesNotAdjacent(
                            previousWidth.TemporaryId,
                            previousWidth.To,
                            width.TemporaryId,
                            width.From).ToValidationException();
                    }

                    if (width.From == width.To)
                    {
                        throw new RoadSegmentWidthAttributeHasLengthOfZero(
                            width.TemporaryId,
                            width.From,
                            width.To).ToValidationException();
                    }
                }

                previousWidth = width;
            }

            if (previousWidth != null)
            {
                if (Math.Abs(previousWidth.To.ToDouble() - line.Length) > context.InstanceToValidate.BeforeVerificationContext.Tolerances.DynamicRoadSegmentAttributePositionTolerance)
                {
                    throw new RoadSegmentWidthAttributeToPositionNotEqualToLength(
                        previousWidth.TemporaryId,
                        previousWidth.To,
                        line.Length).ToValidationException();
                }
            }

            return true;
        }

        public override string Name => nameof(WidthsValidator);
    }
}
