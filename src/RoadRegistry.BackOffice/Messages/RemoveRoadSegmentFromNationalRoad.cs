namespace RoadRegistry.BackOffice.Messages;

public class RemoveRoadSegmentFromNationalRoad
{
    public int SegmentId { get; set; }
    public string SegmentGeometryDrawMethod { get; set; }
    public int AttributeId { get; set; }
    public string Number { get; set; }
}
