namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
{
    [Key(1)] public int[] ReusableAttributeIdentifiers { get; set; }
    [Key(0)] public int SegmentId { get; set; }
}