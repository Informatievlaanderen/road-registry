using System;

namespace RoadRegistry.Model
{
    public abstract class DynamicRoadSegmentAttribute
    {
        protected DynamicRoadSegmentAttribute(
            AttributeId id,
            RoadSegmentPosition from,
            RoadSegmentPosition to,
            GeometryVersion asOfGeometryVersion
        )
        {
            if(from >= to)
            {
                throw new ArgumentException(nameof(From),
                $"The from position ({from.ToDecimal()}) must be less than the to position ({to.ToDecimal()}).");
            }

            Id = id;
            From = from;
            To = to;
            AsOfGeometryVersion = asOfGeometryVersion;
        }

        public AttributeId Id { get; }

        public RoadSegmentPosition From { get; }

        public RoadSegmentPosition To { get; }

        public GeometryVersion AsOfGeometryVersion { get; }
    }
}
