namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class RoadNetworkSnapshotSegmentSurfaceAttribute
{
    [Key(0)] public decimal FromPosition { get; set; }
    [Key(1)] public decimal ToPosition { get; set; }
    [Key(2)] public int Type { get; set; }
}
