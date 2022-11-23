namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RenameOrganization")]
[EventDescription("Change the name of an existing organization.")]
public class RenameOrganization : IMessage
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string When { get; set; }
}
