namespace RoadRegistry.BackOffice.Messages;

public class RoadNodeModified
{
    public int Id { get; set; }
    public string Type { get; set; }
    public RoadNodeGeometry Geometry { get; set; }
}
