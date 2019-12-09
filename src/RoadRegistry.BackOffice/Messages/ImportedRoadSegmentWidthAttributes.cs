namespace RoadRegistry.BackOffice.Messages
{
    public class ImportedRoadSegmentWidthAttributes
    {
        public int AttributeId { get; set; }
        public int Width { get; set; }
        public decimal FromPosition { get; set; }
        public decimal ToPosition { get; set; }
        public int AsOfGeometryVersion { get; set; }
        public ImportedOriginProperties Origin { get; set;}
    }
}
