namespace RoadRegistry.Model
{
    public class RoadSegmentLaneAttribute : DynamicRoadSegmentAttribute
    {
        public RoadSegmentLaneAttribute(
            LaneCount count,
            LaneDirection direction,
            Position from,
            Position to,
            GeometryVersion asOfGeometryVersion
        ) : base(from, to, asOfGeometryVersion)
        {
            Count = count;
            Direction = direction;
        }

        public LaneCount Count { get; }
        public LaneDirection Direction { get; }
    }
}