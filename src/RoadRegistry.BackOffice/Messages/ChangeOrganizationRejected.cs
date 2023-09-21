namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("ChangeOrganizationRejected")]
[EventDescription("Notifies the domain that the request to change an organization has been rejected.")]
public class ChangeOrganizationRejected : IMessage
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string OvoCode { get; set; }
    public string When { get; set; }
}
