namespace RoadRegistry.Messages
{
    public class ImportedRoadSegmentLaneAttributes
    {
        public int AttributeId { get; set; }
        public int Count { get; set; }
        public string Direction { get; set; }
        public decimal FromPosition { get; set; }
        public decimal ToPosition { get; set; }
        public int AsOfGeometryVersion { get; set; }
        public OriginProperties Origin { get; set; }
    }
}
