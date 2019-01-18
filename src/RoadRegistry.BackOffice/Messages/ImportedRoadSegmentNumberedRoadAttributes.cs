namespace RoadRegistry.BackOffice.Messages
{
    public class ImportedRoadSegmentNumberedRoadAttributes
    {
        public int AttributeId { get; set; }
        public string Ident8 { get; set; }
        public string Direction { get; set; }
        public int Ordinal { get; set; }
        public OriginProperties Origin { get; set; }
    }
}
