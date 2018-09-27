namespace RoadRegistry.Model
{
    public class RoadSegmentWidthAttribute : DynamicRoadSegmentAttribute
    {
        public RoadSegmentWidthAttribute(
            RoadSegmentWidth width,
            RoadSegmentPosition from, 
            RoadSegmentPosition to, 
            GeometryVersion asOfGeometryVersion
        ) : base(from, to, asOfGeometryVersion)
        {
            Width = width;
        }

        public RoadSegmentWidth Width { get; }
    }
}