namespace RoadRegistry.Messages
{
    public class RoadSegmentAddedToEuropeanRoad
    {
        public int AttributeId { get; set; }
        public int TemporaryAttributeId { get; set; }
        public string RoadNumber { get; set; }
        public int RoadSegmentId { get; set; }
    }
}
