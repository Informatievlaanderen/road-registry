namespace RoadRegistry.Events
{
    using System;
    using Aiv.Vbr.EventHandling;
    using Shared;

    [EventName("ImportedRoadSegment")]
    [EventDescription("Indicates a road network segment was imported.")]
    public class ImportedRoadSegment
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public int StartNodeId { get; set; }
        public int EndNodeId { get; set; }
        public byte[] Geometry { get; set; }
        public int GeometryVersion { get; set; }
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
        public DateTime RecordingDate { get; set; }
        public OriginProperties Origin { get; set; }
    }
}
