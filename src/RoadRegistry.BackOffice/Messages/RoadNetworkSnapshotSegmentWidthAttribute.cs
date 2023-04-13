namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class RoadNetworkSnapshotSegmentWidthAttribute
{
    [Key(0)] public decimal FromPosition { get; set; }
    [Key(1)] public decimal ToPosition { get; set; }
    [Key(2)] public int Width { get; set; }
    [Key(3)] public int AsOfGeometryVersion { get; set; }
}
