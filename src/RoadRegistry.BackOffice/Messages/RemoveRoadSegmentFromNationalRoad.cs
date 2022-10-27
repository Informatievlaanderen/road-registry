namespace RoadRegistry.BackOffice.Messages;

public class RemoveRoadSegmentFromNationalRoad
{
    public int AttributeId { get; set; }
    public string Number { get; set; }
    public int SegmentId { get; set; }
}