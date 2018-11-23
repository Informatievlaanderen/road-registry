namespace RoadRegistry.Model
{
    public class RoadSegmentWidthAttribute : DynamicRoadSegmentAttribute
    {
        public RoadSegmentWidthAttribute(
            AttributeId id,
            RoadSegmentWidth width,
            RoadSegmentPosition from,
            RoadSegmentPosition to,
            GeometryVersion asOfGeometryVersion
        ) : base(id, from, to, asOfGeometryVersion)
        {
            Width = width;
        }

        public RoadSegmentWidth Width { get; }
    }
}
