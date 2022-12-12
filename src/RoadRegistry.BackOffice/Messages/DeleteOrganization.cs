namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("DeleteOrganization")]
[EventDescription("Indicates that a deletion of an organization got requested")]
public class DeleteOrganization : IMessage
{
    public string Code { get; set; }
}
