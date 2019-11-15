namespace RoadRegistry.BackOffice.Messages
{
    using MessagePack;

    [MessagePackObject]
    public class RoadNetworkSnapshot
    {
        [Key(0)]
        public RoadNetworkSnapshotNode[] Nodes { get; set; }
        [Key(1)]
        public RoadNetworkSnapshotSegment[] Segments { get; set; }
        [Key(2)]
        public int MaximumNodeId { get; set; }
        [Key(3)]
        public int MaximumSegmentId { get; set; }
        [Key(4)]
        public int MaximumGradeSeparatedJunctionId { get; set; }
        [Key(5)]
        public int MaximumEuropeanRoadAttributeId { get; set; }
        [Key(6)]
        public int MaximumNationalRoadAttributeId { get; set; }
        [Key(7)]
        public int MaximumNumberedRoadAttributeId { get; set; }
        [Key(8)]
        public int MaximumLaneAttributeId { get; set; }
        [Key(9)]
        public int MaximumWidthAttributeId { get; set; }
        [Key(10)]
        public int MaximumSurfaceAttributeId { get; set; }
        [Key(11)]
        public RoadNetworkSnapshotSegmentReusableAttributeIdentifiers[] SegmentReusableLaneAttributeIdentifiers { get; set; }
        [Key(12)]
        public RoadNetworkSnapshotSegmentReusableAttributeIdentifiers[] SegmentReusableWidthAttributeIdentifiers { get; set; }
        [Key(13)]
        public RoadNetworkSnapshotSegmentReusableAttributeIdentifiers[] SegmentReusableSurfaceAttributeIdentifiers { get; set; }
    }
}
