namespace RoadRegistry.Messages
{
    public class RemoveRoadSegmentFromEuropeanRoad
    {
        public int AttributeId { get; set; }
        public int SegmentId { get; set; }
        public string RoadNumber { get; set; }
    }
}
