namespace RoadRegistry.Model
{
    public class RoadSegmentLaneAttribute : DynamicRoadSegmentAttribute
    {
        public RoadSegmentLaneAttribute(
            RoadSegmentLaneCount count,
            RoadSegmentLaneDirection direction,
            RoadSegmentPosition from,
            RoadSegmentPosition to,
            GeometryVersion asOfGeometryVersion
        ) : base(from, to, asOfGeometryVersion)
        {
            Count = count;
            Direction = direction;
        }

        public RoadSegmentLaneCount Count { get; }
        public RoadSegmentLaneDirection Direction { get; }
    }
}