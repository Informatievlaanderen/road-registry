namespace RoadRegistry.Model
{
    public class RoadSegmentLaneAttribute : DynamicRoadSegmentAttribute
    {
        public RoadSegmentLaneAttribute(
            AttributeId id,
            RoadSegmentLaneCount count,
            RoadSegmentLaneDirection direction,
            RoadSegmentPosition from,
            RoadSegmentPosition to,
            GeometryVersion asOfGeometryVersion
        ) : base(id, from, to, asOfGeometryVersion)
        {
            Count = count;
            Direction = direction;
        }

        public RoadSegmentLaneCount Count { get; }
        public RoadSegmentLaneDirection Direction { get; }
    }
}
