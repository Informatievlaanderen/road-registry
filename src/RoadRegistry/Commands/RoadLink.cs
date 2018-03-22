namespace RoadRegistry.Commands
{
    public class RoadLink
    {
        public int Id { get; set; }
        public int StartNodeId { get; set; }
        public int EndNodeId { get; set; }
        public byte[] Geometry { get; set; }
        public RoadLinkMaintenanceAuthority Maintainer { get; set; }
        public RoadLinkGeometryDrawMethod GeometryDrawMethod { get; set; }
        public RoadLinkMorphology Morphology { get; set; }
        public RoadLinkStatus Status { get; set; }
        public RoadLinkCategory Category { get; set; }
        public RoadLinkAccessRestriction AccessRestriction { get; set; }
        public RoadLinkSideProperties LeftSide { get; set; }
        public RoadLinkSideProperties RightSide { get; set; }
        public RoadLinkOrigin Origin { get; set; }
    }
}