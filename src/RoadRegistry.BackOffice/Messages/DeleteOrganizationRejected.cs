namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("DeleteOrganizationRejected")]
[EventDescription("Notifies the domain that the request to delete an organization has been rejected.")]
public class DeleteOrganizationRejected : IMessage, IWhen
{
    public string Code { get; set; }
    public string When { get; set; }
}
