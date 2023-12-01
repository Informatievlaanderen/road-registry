namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("NoRoadNetworkChanges")]
[EventDescription("Indicates that there were no road network changes.")]
public class NoRoadNetworkChanges : IMessage, IHaveTransactionId
{
    public string Operator { get; set; }
    public string Organization { get; set; }
    public string OrganizationId { get; set; }
    public string Reason { get; set; }
    public string RequestId { get; set; }
    public int TransactionId { get; set; }
    public string When { get; set; }
}
