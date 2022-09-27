namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class RoadSegmentGeometry
{
    [Key(0)] public int SpatialReferenceSystemIdentifier { get; set; }

    [Key(1)] public LineString[] MultiLineString { get; set; }
}
