namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class RoadNodeGeometry
{
    [Key(1)] public Point Point { get; set; }
    [Key(0)] public int SpatialReferenceSystemIdentifier { get; set; }
}