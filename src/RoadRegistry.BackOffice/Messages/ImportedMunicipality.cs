namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("ImportedMunicipality")]
[EventDescription("Indicates a municipality was imported.")]
public class ImportedMunicipality : IMessage
{
    public string DutchName { get; set; }
    public MunicipalityGeometry Geometry { get; set; }
    public string NISCode { get; set; }
    public string When { get; set; }
}
