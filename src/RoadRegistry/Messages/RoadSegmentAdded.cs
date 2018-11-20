namespace RoadRegistry.Messages
{
    using System;

    public class RoadSegmentAdded
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public int StartNodeId { get; set; }
        public int EndNodeId { get; set; }
        [Obsolete("Please use Geometry2 instead.")]
        public byte[] Geometry { get; set; }
        public RoadSegmentGeometry Geometry2 { get; set; }
        public int GeometryVersion { get; set; }
        public MaintenanceAuthority MaintenanceAuthority { get; set; }
        public string GeometryDrawMethod { get; set; }
        public string Morphology { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string AccessRestriction { get; set; }
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
