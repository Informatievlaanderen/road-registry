namespace RoadRegistry.BackOffice.Messages;

public class RequestedRoadSegmentNumberedRoadAttribute
{
    public int AttributeId { get; set; }
    public string Number { get; set; }
    public string Direction { get; set; }
    public int Ordinal { get; set; }
}
