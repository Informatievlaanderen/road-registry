namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("ChangeOrganization")]
[EventDescription("Indicates that a change of an organization got requested")]
public class ChangeOrganization : IMessage
{
    public string Code { get; set; }
    public string? Name { get; set; }
    public string? OvoCode { get; set; }
    public string? KboNumber { get; set; }
    public bool? IsMaintainer { get; set; }
}
