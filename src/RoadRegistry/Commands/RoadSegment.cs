namespace RoadRegistry.Commands
{
    public class RoadSegment
    {
        public int Id { get; set; }
        public int SourceNodeId { get; set; }
        public int TargetNodeId { get; set; }
        public byte[] Geometry { get; set; }
        public RoadSegmentMaintainer Maintainer { get; set; }
        public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; set; }
        public RoadSegmentMorphology Morphology { get; set; }
        public RoadSegmentStatus Status { get; set; }
        public RoadSegmentCategory Category { get; set; }
        public RoadSegmentAccessRestriction AccessRestriction { get; set; }
        public RoadSegmentSide LeftSide { get; set; }
        public RoadSegmentSide RightSide { get; set; }
        public RoadSegmentOrigin Origin { get; set; }
    }
}