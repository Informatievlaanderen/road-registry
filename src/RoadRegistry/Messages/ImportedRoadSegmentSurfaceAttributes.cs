namespace RoadRegistry.Messages
{
    public class ImportedRoadSegmentSurfaceAttributes
    {
        public int AttributeId { get; set; }
        public SurfaceType Type { get; set; }
        public decimal FromPosition { get; set; }
        public decimal ToPosition { get; set; }
        public int AsOfGeometryVersion { get; set; }
        public OriginProperties Origin { get; set; }
    }
}
