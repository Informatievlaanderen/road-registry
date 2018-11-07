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
        public ImportedRoadSegmentSideProperties LeftSide { get; set; }
        public ImportedRoadSegmentSideProperties RightSide { get; set; }
        public ImportedRoadSegmentEuropeanRoadProperties[] PartOfEuropeanRoads { get; set; }
        public ImportedRoadSegmentNationalRoadProperties[] PartOfNationalRoads { get; set; }
        public ImportedRoadSegmentNumberedRoadProperties[] PartOfNumberedRoads { get; set; }
        public ImportedRoadSegmentLaneProperties[] Lanes { get; set; }
        public ImportedRoadSegmentWidthProperties[] Widths { get; set; }
        public ImportedRoadSegmentHardeningProperties[] Hardenings { get; set; }
        public DateTime RecordingDate { get; set; }
        public OriginProperties Origin { get; set; }
    }
}
