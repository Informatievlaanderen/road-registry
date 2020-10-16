namespace RoadRegistry.BackOffice.Messages
{
    public class ImportedRoadSegmentNumberedRoadAttribute
    {
        public int AttributeId { get; set; }
        public string Number { get; set; }
        public string Direction { get; set; }
        public int Ordinal { get; set; }
        public ImportedOriginProperties Origin { get; set; }
    }
}
