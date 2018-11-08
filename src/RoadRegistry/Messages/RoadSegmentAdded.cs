namespace RoadRegistry.Messages
{
    using System;

    public class RoadSegmentAdded
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
        public RoadSegmentSideAttributes LeftSide { get; set; }
        public RoadSegmentSideAttributes RightSide { get; set; }
        public RoadSegmentEuropeanRoadAttributes[] PartOfEuropeanRoads { get; set; }
        public RoadSegmentNationalRoadAttributes[] PartOfNationalRoads { get; set; }
        public RoadSegmentNumberedRoadAttributes[] PartOfNumberedRoads { get; set; }
        public RoadSegmentLaneAttributes[] Lanes { get; set; }
        public RoadSegmentWidthAttributes[] Widths { get; set; }
        public RoadSegmentSurfaceAttributes[] Surfaces { get; set; }
        public DateTime RecordingDate { get; set; }
        public OriginProperties Origin { get; set; }
    }
}
