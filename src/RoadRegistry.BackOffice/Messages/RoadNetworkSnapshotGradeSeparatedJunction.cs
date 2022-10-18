namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class RoadNetworkSnapshotGradeSeparatedJunction
{
    [Key(0)] public int Id { get; set; }

    [Key(2)] public int LowerSegmentId { get; set; }

    [Key(3)] public string Type { get; set; }

    [Key(1)] public int UpperSegmentId { get; set; }
}