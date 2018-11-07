namespace RoadRegistry.Events
{
    using Shared;

    public class ImportedRoadSegmentNumberedRoadProperties
    {
        public int AttributeId { get; set; }
        public string Ident8 { get; set; }
        public NumberedRoadSegmentDirection Direction { get; set; }        
        public int Ordinal { get; set; }
        public OriginProperties Origin { get; set; }
    }
}