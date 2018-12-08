namespace RoadRegistry.Messages
{
    public class RequestedRoadSegmentSurfaceAttribute
    {
        public int AttributeId { get; set; }
        public string Type { get; set; }
        public decimal FromPosition { get; set; }
        public decimal ToPosition { get; set; }
    }
}
