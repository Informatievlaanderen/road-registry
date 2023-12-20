namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("CreateOrganizationAccepted")]
[EventDescription("Notifies the domain that an organization has been created.")]
public class CreateOrganizationAccepted : IMessage
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string OvoCode { get; set; }
    public string When { get; set; }
}
