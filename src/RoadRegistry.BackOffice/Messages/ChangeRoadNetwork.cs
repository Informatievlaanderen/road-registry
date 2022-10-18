namespace RoadRegistry.BackOffice.Messages;

public class ChangeRoadNetwork
{
    public RequestedChange[] Changes { get; set; }
    public string Operator { get; set; }
    public string OrganizationId { get; set; }
    public string Reason { get; set; }
    public string RequestId { get; set; }
}