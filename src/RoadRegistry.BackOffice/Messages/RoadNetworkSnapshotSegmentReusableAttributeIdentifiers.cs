namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
{
    [Key(0)] public int SegmentId { get; set; }

    [Key(1)] public int[] ReusableAttributeIdentifiers { get; set; }
}
