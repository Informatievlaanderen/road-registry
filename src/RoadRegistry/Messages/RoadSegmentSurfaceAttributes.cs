namespace RoadRegistry.Messages
{
    public class RoadSegmentSurfaceAttributes
    {
        public int AttributeId { get; set; }
        public SurfaceType Type { get; set; }
        public decimal FromPosition { get; set; }
        public decimal ToPosition { get; set; }
        public int AsOfGeometryVersion { get; set; }
    }
}
