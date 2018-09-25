using RoadRegistry.Shared;

namespace RoadRegistry.Commands
{
    public class AddRoadSegment
    {
        public int Id { get; set; }
        public int StartNodeId { get; set; }
        public int EndNodeId { get; set; }
        public byte[] Geometry { get; set; }
        public Maintainer Maintainer { get; set; }
        public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; set; }
        public RoadSegmentMorphology Morphology { get; set; }
        public RoadSegmentStatus Status { get; set; }
        public RoadSegmentCategory Category { get; set; }
        public RoadSegmentAccessRestriction AccessRestriction { get; set; }
        public RoadSegmentSideProperties LeftSide { get; set; }
        public RoadSegmentSideProperties RightSide { get; set; }
        public RoadSegmentEuropeanRoadProperties[] PartOfEuropeanRoads { get; set; }
        public RoadSegmentNationalRoadProperties[] PartOfNationalRoads { get; set; }
        public RoadSegmentNumberedRoadProperties[] PartOfNumberedRoads { get; set; }
        public RoadSegmentLaneProperties[] Lanes { get; set; }
        public RoadSegmentWidthProperties[] Widths { get; set; }
        public RoadSegmentHardeningProperties[] Hardenings { get; set; }
    }

    public class RoadSegmentSideProperties
    {
        public int? StreetNameId { get; set; }
    }

    public class RoadSegmentEuropeanRoadProperties
    {
        public int AttributeId { get; set; }
        public string RoadNumber { get; set; }
    }

    public class RoadSegmentHardeningProperties
    {
        public int AttributeId { get; set; }
        public HardeningType Type { get; set; }
        public decimal FromPosition { get; set; }
        public decimal ToPosition { get; set; }
    }

    public class RoadSegmentLaneProperties
    {
        public int AttributeId { get; set; }
        public int Count { get; set; }
        public LaneDirection Direction { get; set; }
        public decimal FromPosition { get; set; }
        public decimal ToPosition { get; set; }
    }
    
    public class RoadSegmentNationalRoadProperties
    {
        public int AttributeId { get; set; }
        public string Ident2 { get; set; }
    }
}
