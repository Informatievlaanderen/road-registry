namespace RoadRegistry.Model
{
    public class RoadSegmentWidthAttribute : DynamicRoadSegmentAttribute
    {
        public RoadSegmentWidthAttribute(
            Width width,
            Position from, 
            Position to, 
            GeometryVersion asOfGeometryVersion
        ) : base(from, to, asOfGeometryVersion)
        {
            Width = width;
        }

        public Width Width { get; }
    }
}