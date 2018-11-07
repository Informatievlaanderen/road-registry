namespace RoadRegistry.Messages
{
    public class RequestedRoadSegmentHardeningProperties
    {
        public HardeningType Type { get; set; }
        public double FromPosition { get; set; }
        public double ToPosition { get; set; }
    }
}
