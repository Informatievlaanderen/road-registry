namespace RoadRegistry.Messages
{
    public class RoadSegmentWidthAttributes
    {
        public int AttributeId { get; set; }
        public int Width { get; set; }
        public decimal FromPosition { get; set; }
        public decimal ToPosition { get; set; }
        public int AsOfGeometryVersion { get; set; }
    }
}
