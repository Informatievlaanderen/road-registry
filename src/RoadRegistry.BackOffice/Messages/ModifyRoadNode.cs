namespace RoadRegistry.BackOffice.Messages;

public class ModifyRoadNode
{
    public int Id { get; set; }
    public string? Type { get; set; }
    public RoadNodeGeometry? Geometry { get; set; }
}
