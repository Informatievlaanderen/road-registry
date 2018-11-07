namespace RoadRegistry.Messages
{
    public class ImportedRoadSegmentEuropeanRoadProperties
    {
        public int AttributeId { get; set; }
        public string RoadNumber { get; set; }
        public OriginProperties Origin { get; set; }
    }
}