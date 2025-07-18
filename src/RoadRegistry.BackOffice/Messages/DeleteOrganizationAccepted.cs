namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("DeleteOrganizationAccepted")]
[EventDescription("Notifies the domain that an organization has been deleted.")]
public class DeleteOrganizationAccepted : IMessage, IWhen
{
    public string Code { get; set; }
    public string When { get; set; }
}
