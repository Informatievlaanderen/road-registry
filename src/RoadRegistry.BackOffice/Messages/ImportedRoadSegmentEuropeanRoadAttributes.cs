namespace RoadRegistry.BackOffice.Messages
{
    public class ImportedRoadSegmentEuropeanRoadAttributes
    {
        public int AttributeId { get; set; }
        public string Number { get; set; }
        public ImportedOriginProperties Origin { get; set; }
    }
}
