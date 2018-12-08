namespace RoadRegistry.Messages
{
    public class RemoveRoadSegmentFromEuropeanRoad
    {
        public int AttributeId { get; set; }
        public string RoadNumber { get; set; }
        public int RoadSegmentId { get; set; }
    }
}
