namespace RoadRegistry.Messages
{
    public class RoadSegmentNumberedRoadAttributes
    {
        public int AttributeId { get; set; }
        public string Ident8 { get; set; }
        public NumberedRoadSegmentDirection Direction { get; set; }
        public int Ordinal { get; set; }
    }
}
