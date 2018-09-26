using System;

namespace RoadRegistry.Model
{
    public abstract class DynamicRoadSegmentAttribute
    {
        protected DynamicRoadSegmentAttribute(
            Position from,
            Position to,
            GeometryVersion asOfGeometryVersion
        )
        {
            if(from >= to)
            {
                throw new ArgumentException(nameof(From), 
                $"The from position ({from.ToDouble()}) must be less than the to position ({to.ToDouble()}).");
            }
        }

        public Position From { get; }

        public Position To { get; }

        public GeometryVersion AsOfGeometryVersion { get; }
    }
}