namespace RoadRegistry.Events
{
    public class ImportedRoadSegmentNationalRoadProperties
    {
        public int AttributeId { get; set; }
        public string Ident2 { get; set; }
        public OriginProperties Origin { get; set; }        
    }
}