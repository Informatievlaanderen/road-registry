namespace RoadRegistry.BackOffice.Messages;

public class RoadNodeModified
{
    public RoadNodeGeometry Geometry { get; set; }
    public int Id { get; set; }
    public string Type { get; set; }
}
