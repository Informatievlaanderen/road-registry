namespace RoadRegistry.Model
{
    public class RoadSegmentSurfaceAttribute : DynamicRoadSegmentAttribute
    {
        public RoadSegmentSurfaceAttribute(
            RoadSegmentSurfaceType type,
            RoadSegmentPosition from, 
            RoadSegmentPosition to, 
            GeometryVersion asOfGeometryVersion
        ) : base(from, to, asOfGeometryVersion)
        {
            Type = type;
        }

        public RoadSegmentSurfaceType Type { get; }
    }
}