namespace RoadRegistry.BackOffice.Messages
{
    public class RoadSegmentAdded
    {
        public int Id { get; set; }
        public int TemporaryId { get; set;  }
        public int Version { get; set; }
        public int StartNodeId { get; set; }
        public int EndNodeId { get; set; }
        public RoadSegmentGeometry Geometry { get; set; }
        public int GeometryVersion { get; set; }
        public MaintenanceAuthority MaintenanceAuthority { get; set; }
        public string GeometryDrawMethod { get; set; }
        public string Morphology { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string AccessRestriction { get; set; }
        public RoadSegmentSideAttributes LeftSide { get; set; }
        public RoadSegmentSideAttributes RightSide { get; set; }
        public RoadSegmentLaneAttributes[] Lanes { get; set; }
        public RoadSegmentWidthAttributes[] Widths { get; set; }
        public RoadSegmentSurfaceAttributes[] Surfaces { get; set; }
    }
}
