namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class RoadNetworkSnapshotHead
{
    [Key(0)] public string SnapshotBlobName { get; set; }
}