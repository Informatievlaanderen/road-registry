namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RenameOrganizationAccepted")]
[EventDescription("Notifies the domain that the organization has been renamed.")]
public class RenameOrganizationAccepted : IMessage, IWhen
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string When { get; set; }
}
