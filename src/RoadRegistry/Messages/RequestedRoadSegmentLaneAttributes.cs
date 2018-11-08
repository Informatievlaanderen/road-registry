namespace RoadRegistry.Messages
{
    public class RequestedRoadSegmentLaneAttributes
    {
        public int Count { get; set; }
        public LaneDirection Direction { get; set; }
        public decimal FromPosition { get; set; }
        public decimal ToPosition { get; set; }
    }
}
