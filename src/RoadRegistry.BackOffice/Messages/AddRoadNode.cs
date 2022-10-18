namespace RoadRegistry.BackOffice.Messages;

public class AddRoadNode
{
    public RoadNodeGeometry Geometry { get; set; }
    public int TemporaryId { get; set; }
    public string Type { get; set; }
}