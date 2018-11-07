namespace RoadRegistry.Messages
{
    public class AddRoadSegment
    {
        public int Id { get; set; }
        public int StartNodeId { get; set; }
        public int EndNodeId { get; set; }
        public byte[] Geometry { get; set; }
        public string Maintainer { get; set; }
        public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; set; }
        public RoadSegmentMorphology Morphology { get; set; }
        public RoadSegmentStatus Status { get; set; }
        public RoadSegmentCategory Category { get; set; }
        public RoadSegmentAccessRestriction AccessRestriction { get; set; }
        public int? LeftSideStreetNameId { get; set; }
        public int? RightSideStreetNameId { get; set; }
        public RequestedRoadSegmentEuropeanRoadProperties[] PartOfEuropeanRoads { get; set; }
        public RequestedRoadSegmentNationalRoadProperties[] PartOfNationalRoads { get; set; }
        public RequestedRoadSegmentNumberedRoadProperties[] PartOfNumberedRoads { get; set; }
        public RequestedRoadSegmentLaneProperties[] Lanes { get; set; }
        public RequestedRoadSegmentWidthProperties[] Widths { get; set; }
        public RequestedRoadSegmentHardeningProperties[] Hardenings { get; set; }
    }
}
