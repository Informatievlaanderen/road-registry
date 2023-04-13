namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class RoadNetworkSnapshotSegmentLaneAttribute
{
    [Key(0)] public decimal FromPosition { get; set; }
    [Key(1)] public decimal ToPosition { get; set; }
    [Key(2)] public int Count { get; set; }
    [Key(3)] public int Direction { get; set; }
    [Key(4)] public int AsOfGeometryVersion { get; set; }
}
