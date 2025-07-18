namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("ChangeOrganizationAccepted")]
[EventDescription("Notifies the domain that the organization has been changed.")]
public class ChangeOrganizationAccepted : IMessage, IWhen
{
    public string Code { get; set; }
    public string Name { get; set; }
    public bool NameModified { get; set; }
    public string OvoCode { get; set; }
    public bool OvoCodeModified { get; set; }
    public string KboNumber { get; set; }
    public bool KboNumberModified { get; set; }
    public bool IsMaintainer { get; set; }
    public bool IsMaintainerModified { get; set; }
    public string When { get; set; }
}
