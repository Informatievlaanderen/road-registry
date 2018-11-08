namespace RoadRegistry.Messages
{
    using System;
    using Aiv.Vbr.EventHandling;

    [EventName("ImportedRoadSegment")]
    [EventDescription("Indicates a road network segment was imported.")]
    public class ImportedRoadSegment
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public int StartNodeId { get; set; }
        public int EndNodeId { get; set; }
        public byte[] Geometry { get; set; }
        public int GeometryVersion { get; set; }
        public MaintenanceAuthority MaintenanceAuthority { get; set; }
        public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; set; }
        public RoadSegmentMorphology Morphology { get; set; }
        public RoadSegmentStatus Status { get; set; }
        public RoadSegmentCategory Category { get; set; }
        public RoadSegmentAccessRestriction AccessRestriction { get; set; }
        public ImportedRoadSegmentSideAttributes LeftSide { get; set; }
        public ImportedRoadSegmentSideAttributes RightSide { get; set; }
        public ImportedRoadSegmentEuropeanRoadAttributes[] PartOfEuropeanRoads { get; set; }
        public ImportedRoadSegmentNationalRoadAttributes[] PartOfNationalRoads { get; set; }
        public ImportedRoadSegmentNumberedRoadAttributes[] PartOfNumberedRoads { get; set; }
        public ImportedRoadSegmentLaneAttributes[] Lanes { get; set; }
        public ImportedRoadSegmentWidthAttributes[] Widths { get; set; }
        public ImportedRoadSegmentSurfaceAttributes[] Surfaces { get; set; }
        public DateTime RecordingDate { get; set; }
        public OriginProperties Origin { get; set; }
    }
}
