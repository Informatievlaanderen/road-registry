namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("ChangeOrganizationAccepted")]
[EventDescription("Notifies the domain that the organization has been changed.")]
public class ChangeOrganizationAccepted : IMessage
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string OvoCode { get; set; }
    public string When { get; set; }
}
