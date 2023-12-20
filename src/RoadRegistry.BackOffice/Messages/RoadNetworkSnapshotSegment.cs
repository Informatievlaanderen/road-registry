namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class RoadNetworkSnapshotSegment
{
    [Key(0)] public int Id { get; set; }
    [Key(1)] public int StartNodeId { get; set; }
    [Key(2)] public int EndNodeId { get; set; }
    [Key(3)] public RoadSegmentGeometry Geometry { get; set; }
    [Key(4)] public RoadNetworkSnapshotSegmentAttributeHash AttributeHash { get; set; }
    [Key(5)] public RoadNetworkSnapshotSegmentEuropeanRoadAttribute[] EuropeanRoadAttributes { get; set; }
    [Key(6)] public RoadNetworkSnapshotSegmentNationalRoadAttribute[] NationalRoadAttributes { get; set; }
    [Key(7)] public RoadNetworkSnapshotSegmentNumberedRoadAttribute[] NumberedRoadAttributes { get; set; }
    [Key(8)] public string LastEventHash { get; set; }
    [Key(9)] public int Version { get; set; }
    [Key(10)] public int GeometryVersion { get; set; }
    [Key(11)] public RoadNetworkSnapshotSegmentLaneAttribute[] Lanes { get; set; }
    [Key(12)] public RoadNetworkSnapshotSegmentSurfaceAttribute[] Surfaces { get; set; }
    [Key(13)] public RoadNetworkSnapshotSegmentWidthAttribute[] Widths { get; set; }
}
