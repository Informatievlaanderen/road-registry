namespace RoadRegistry.Messages
{
    public class RequestedRoadSegmentSurfaceAttributes
    {
        public string Type { get; set; }
        public decimal FromPosition { get; set; }
        public decimal ToPosition { get; set; }
    }
}
