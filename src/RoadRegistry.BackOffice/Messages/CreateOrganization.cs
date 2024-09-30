namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

public class CreateOrganization : IMessage
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string OvoCode { get; set; }
    public string KboNumber { get; set; }
}
