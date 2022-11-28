namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

public class ChangeRoadNetwork : IMessage
{
    public RequestedChange[] Changes { get; set; }
    public string Operator { get; set; }
    public string OrganizationId { get; set; }
    public string Reason { get; set; }
    public string RequestId { get; set; }
}
