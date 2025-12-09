namespace RoadRegistry.RoadNode.Events.V1.ValueObjects;

public class RoadNodeGeometry
{
    public required string WKT { get; set; }
    public required int SpatialReferenceSystemIdentifier { get; set; }
}
