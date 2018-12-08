namespace RoadRegistry.Messages
{
    public class RemoveSegmentFromNationalRoad
    {
        public int AttributeId { get; set; }
        public string Ident2 { get; set; }
        public int RoadSegmentId { get; set; }
    }
}