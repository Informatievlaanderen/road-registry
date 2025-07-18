namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RenameOrganizationRejected")]
[EventDescription("Notifies the domain that the request to change an organization's name has been rejected.")]
public class RenameOrganizationRejected : IMessage, IWhen
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string When { get; set; }
}
