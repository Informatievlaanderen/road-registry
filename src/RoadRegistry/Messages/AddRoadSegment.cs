namespace RoadRegistry.Messages
{
    public class AddRoadSegment
    {
        public int TemporaryId { get; set; }
        public int StartNodeId { get; set; }
        public int EndNodeId { get; set; }
        public RoadSegmentGeometry Geometry { get; set; }
        public string MaintenanceAuthority { get; set; }
        public string GeometryDrawMethod { get; set; }
        public string Morphology { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string AccessRestriction { get; set; }
        public int? LeftSideStreetNameId { get; set; }
        public int? RightSideStreetNameId { get; set; }
        public RoadSegmentEuropeanRoadAttributes[] PartOfEuropeanRoads { get; set; }
        public RoadSegmentNationalRoadAttributes[] PartOfNationalRoads { get; set; }
        public RoadSegmentNumberedRoadAttributes[] PartOfNumberedRoads { get; set; }
        public RoadSegmentLaneAttributes[] Lanes { get; set; }
        public RoadSegmentWidthAttributes[] Widths { get; set; }
        public RoadSegmentSurfaceAttributes[] Surfaces { get; set; }
    }
}
