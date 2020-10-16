namespace RoadRegistry.BackOffice.Messages
{
    public class RemoveRoadSegmentFromNumberedRoad
    {
        public int AttributeId { get; set; }
        public int SegmentId { get; set; }
        public string Number { get; set; }
    }
}
