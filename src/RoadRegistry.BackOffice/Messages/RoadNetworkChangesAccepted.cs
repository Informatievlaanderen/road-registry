namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RoadNetworkChangesAccepted")]
[EventDescription("Indicates the road network changes were accepted.")]
public class RoadNetworkChangesAccepted
{
    public AcceptedChange[] Changes { get; set; }
    public string Operator { get; set; }
    public string Organization { get; set; }
    public string OrganizationId { get; set; }
    public string Reason { get; set; }
    public string RequestId { get; set; }
    public int TransactionId { get; set; }
    public string When { get; set; }
}
