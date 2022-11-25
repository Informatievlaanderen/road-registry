namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RenameOrganization")]
[EventDescription("Indicates that a rename of an organization got requested")]
public class RenameOrganization : IMessage
{
    public string Code { get; set; }
    public string Name { get; set; }
}
