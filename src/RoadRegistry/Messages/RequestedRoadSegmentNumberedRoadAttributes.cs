namespace RoadRegistry.Messages
{
    public class RequestedRoadSegmentNumberedRoadAttributes
    {
        public string Ident8 { get; set; }
        public NumberedRoadSegmentDirection Direction { get; set; }
        public int Ordinal { get; set; }
    }
}
