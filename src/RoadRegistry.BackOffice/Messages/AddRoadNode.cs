namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

public class AddRoadNode : IMessage
{
    public RoadNodeGeometry Geometry { get; set; }
    public int TemporaryId { get; set; }
    public int? OriginalId { get; set; }
    public string Type { get; set; }
}
