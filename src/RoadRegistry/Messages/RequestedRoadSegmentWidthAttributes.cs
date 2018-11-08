namespace RoadRegistry.Messages
{
    public class RequestedRoadSegmentWidthAttributes
    {
        public int Width { get; set; }
        public decimal FromPosition { get; set; }
        public decimal ToPosition { get; set; }
    }
}
