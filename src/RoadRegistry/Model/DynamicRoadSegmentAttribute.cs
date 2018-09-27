using System;

namespace RoadRegistry.Model
{
    public abstract class DynamicRoadSegmentAttribute
    {
        protected DynamicRoadSegmentAttribute(
            RoadSegmentPosition from,
            RoadSegmentPosition to,
            GeometryVersion asOfGeometryVersion
        )
        {
            if(from >= to)
            {
                throw new ArgumentException(nameof(From), 
                $"The from position ({from.ToDouble()}) must be less than the to position ({to.ToDouble()}).");
            }

            From = from;
            To = to;
            AsOfGeometryVersion = asOfGeometryVersion;
        }

        public RoadSegmentPosition From { get; }

        public RoadSegmentPosition To { get; }

        public GeometryVersion AsOfGeometryVersion { get; }
    }
}