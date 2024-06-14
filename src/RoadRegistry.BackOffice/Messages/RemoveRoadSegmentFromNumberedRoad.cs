namespace RoadRegistry.BackOffice.Messages;

public class RemoveRoadSegmentFromNumberedRoad
{
    public int SegmentId { get; set; }
    public string SegmentGeometryDrawMethod { get; set; }
    public int AttributeId { get; set; }
    public string Number { get; set; }
}
