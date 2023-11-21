namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class RoadNetworkSnapshot
{
    //TODO-rik test (de)serialization speed met of zonder een key positie bepaald te hebben per prop
    [Key(15)] public RoadNetworkSnapshotGradeSeparatedJunction[] GradeSeparatedJunctions { get; set; }
    [Key(0)] public RoadNetworkSnapshotNode[] Nodes { get; set; }
    [Key(11)] public RoadNetworkSnapshotSegmentReusableAttributeIdentifiers[] SegmentReusableLaneAttributeIdentifiers { get; set; }
    [Key(13)] public RoadNetworkSnapshotSegmentReusableAttributeIdentifiers[] SegmentReusableSurfaceAttributeIdentifiers { get; set; }
    [Key(12)] public RoadNetworkSnapshotSegmentReusableAttributeIdentifiers[] SegmentReusableWidthAttributeIdentifiers { get; set; }
    [Key(1)] public RoadNetworkSnapshotSegment[] Segments { get; set; }
}
