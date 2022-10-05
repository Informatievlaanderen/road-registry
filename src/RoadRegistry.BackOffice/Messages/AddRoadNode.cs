using Be.Vlaanderen.Basisregisters.EventHandling;

namespace RoadRegistry.BackOffice.Messages;

public class AddRoadNode : IMessage
{
    public int TemporaryId { get; set; }
    public string Type { get; set; }
    public RoadNodeGeometry Geometry { get; set; }
}
