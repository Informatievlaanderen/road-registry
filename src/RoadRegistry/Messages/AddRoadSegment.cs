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
        public RequestedRoadSegmentEuropeanRoadAttributes[] PartOfEuropeanRoads { get; set; }
        public RequestedRoadSegmentNationalRoadAttributes[] PartOfNationalRoads { get; set; }
        public RequestedRoadSegmentNumberedRoadAttributes[] PartOfNumberedRoads { get; set; }
        public RequestedRoadSegmentLaneAttributes[] Lanes { get; set; }
        public RequestedRoadSegmentWidthAttributes[] Widths { get; set; }
        public RequestedRoadSegmentSurfaceAttributes[] Surfaces { get; set; }
    }
}
