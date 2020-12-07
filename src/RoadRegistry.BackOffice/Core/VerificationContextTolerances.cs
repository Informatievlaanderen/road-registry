namespace RoadRegistry.BackOffice.Core
{
    using System;

    public class VerificationContextTolerances
    {
        public VerificationContextTolerances(
            double dynamicRoadSegmentAttributePositionTolerance,
            double measurementTolerance,
            double geometryTolerance)
        {
            if(dynamicRoadSegmentAttributePositionTolerance <= 0.0) throw new ArgumentOutOfRangeException(nameof(dynamicRoadSegmentAttributePositionTolerance));
            if(measurementTolerance <= 0.0) throw new ArgumentOutOfRangeException(nameof(measurementTolerance));
            if(geometryTolerance <= 0.0) throw new ArgumentOutOfRangeException(nameof(geometryTolerance));
            DynamicRoadSegmentAttributePositionTolerance = dynamicRoadSegmentAttributePositionTolerance;
            GeometryTolerance = geometryTolerance;
            MeasurementTolerance = measurementTolerance;
        }

        public double DynamicRoadSegmentAttributePositionTolerance { get; }
        public double MeasurementTolerance { get; }
        public double GeometryTolerance { get; }
    }
}
