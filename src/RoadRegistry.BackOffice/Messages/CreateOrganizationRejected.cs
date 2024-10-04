namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("CreateOrganizationRejected")]
[EventDescription("Notifies the domain that the request to create an organization has been rejected.")]
public class CreateOrganizationRejected : IMessage
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string OvoCode { get; set; }
    public string KboNumber { get; set; }
    public string When { get; set; }
}
