namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("ImportedRoadNode")]
[EventDescription("Indicates a road network node was imported.")]
public class ImportedRoadNode : IMessage, IWhen
{
    public RoadNodeGeometry Geometry { get; set; }
    public int Id { get; set; }
    public ImportedOriginProperties Origin { get; set; }
    public string Type { get; set; }
    public int Version { get; set; }
    public string When { get; set; }
}
