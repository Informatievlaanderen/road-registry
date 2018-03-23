namespace RoadRegistry.Commands
{
    public class RoadSegment
    {
        public int Id { get; set; }
        public int StartNodeId { get; set; }
        public int EndNodeId { get; set; }
        public byte[] Geometry { get; set; }
        public string MaintainerId { get; set; }
        public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; set; }
        public RoadSegmentMorphology Morphology { get; set; }
        public RoadSegmentStatus Status { get; set; }
        public RoadSegmentCategory Category { get; set; }
        public RoadSegmentAccessRestriction AccessRestriction { get; set; }
        public RoadSegmentSideProperties LeftSide { get; set; }
        public RoadSegmentSideProperties RightSide { get; set; }
        public RoadSegmentOrigin Origin { get; set; }
    }
}