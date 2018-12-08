namespace RoadRegistry.Messages
{
    public class RemoveSegmentFromNumberedRoad
    {
        public int AttributeId { get; set; }
        public string Ident8 { get; set; }
        public int RoadSegmentId { get; set; }
    }
}