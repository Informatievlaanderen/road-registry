namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("CreateOrganization")]
[EventDescription("Indicates that a creation of an organization got requested")]
public class CreateOrganization : IMessage
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string OvoCode { get; set; }
}
