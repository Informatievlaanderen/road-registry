namespace RoadRegistry.Messages
{
    public class RequestedRoadSegmentSurfaceAttributes
    {
        public SurfaceType Type { get; set; }
        public decimal FromPosition { get; set; }
        public decimal ToPosition { get; set; }
    }
}
