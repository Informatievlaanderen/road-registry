namespace RoadRegistry.Model
{
    public class RoadSegmentSurfaceAttribute : DynamicRoadSegmentAttribute
    {
        public RoadSegmentSurfaceAttribute(
            AttributeId id,
            RoadSegmentSurfaceType type,
            RoadSegmentPosition from,
            RoadSegmentPosition to,
            GeometryVersion asOfGeometryVersion
        ) : base(id, from, to, asOfGeometryVersion)
        {
            Type = type;
        }

        public RoadSegmentSurfaceType Type { get; }
    }
}
