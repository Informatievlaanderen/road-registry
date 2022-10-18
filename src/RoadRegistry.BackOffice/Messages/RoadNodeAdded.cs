namespace RoadRegistry.BackOffice.Messages;

public class RoadNodeAdded
{
    public RoadNodeGeometry Geometry { get; set; }
    public int Id { get; set; }
    public int TemporaryId { get; set; }
    public string Type { get; set; }
}