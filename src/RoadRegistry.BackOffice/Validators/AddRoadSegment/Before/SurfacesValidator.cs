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

    internal class SurfacesValidator : PropertyValidator<AddRoadSegmentWithBeforeVerificationContext, IReadOnlyList<RoadSegmentSurfaceAttribute>>
    {
        public override bool IsValid(ValidationContext<AddRoadSegmentWithBeforeVerificationContext> context, IReadOnlyList<RoadSegmentSurfaceAttribute> value)
        {
            var line = context.InstanceToValidate.AddRoadSegment.Geometry.Geometries
                .OfType<LineString>()
                .Single();

            RoadSegmentSurfaceAttribute previousSurface = null;
            foreach (var surface in value)
            {
                if (previousSurface == null)
                {
                    if (surface.From != RoadSegmentPosition.Zero)
                    {
                        throw new RoadSegmentSurfaceAttributeFromPositionNotEqualToZero(
                            surface.TemporaryId,
                            surface.From).ToValidationException();
                    }
                }
                else
                {
                    if (surface.From != previousSurface.To)
                    {
                        throw new RoadSegmentSurfaceAttributesNotAdjacent(
                            previousSurface.TemporaryId,
                            previousSurface.To,
                            surface.TemporaryId,
                            surface.From).ToValidationException();
                    }

                    if (surface.From == surface.To)
                    {
                        throw new RoadSegmentSurfaceAttributeHasLengthOfZero(
                            surface.TemporaryId,
                            surface.From,
                            surface.To).ToValidationException();
                    }
                }

                previousSurface = surface;
            }

            if (previousSurface != null)
            {
                if (Math.Abs(previousSurface.To.ToDouble() - line.Length) > context.InstanceToValidate.BeforeVerificationContext.Tolerances.DynamicRoadSegmentAttributePositionTolerance)
                {
                    throw new RoadSegmentSurfaceAttributeToPositionNotEqualToLength(previousSurface.TemporaryId, previousSurface.To, line.Length).ToValidationException();
                }
            }

            return true;
        }

        public override string Name => nameof(SurfacesValidator);
    }
}
