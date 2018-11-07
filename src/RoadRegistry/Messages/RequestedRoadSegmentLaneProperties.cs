namespace RoadRegistry.Messages
{
    public class RequestedRoadSegmentLaneProperties
    {
        public int Count { get; set; }
        public LaneDirection Direction { get; set; }
        public double FromPosition { get; set; }
        public double ToPosition { get; set; }
    }
}
