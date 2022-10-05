using Be.Vlaanderen.Basisregisters.EventHandling;

namespace RoadRegistry.BackOffice.Messages;

public class ChangeRoadNetwork : IMessage
{
    public string RequestId { get; set; }
    public string Reason { get; set; }
    public string Operator { get; set; }
    public string OrganizationId { get; set; }
    public RequestedChange[] Changes { get; set; }
}
