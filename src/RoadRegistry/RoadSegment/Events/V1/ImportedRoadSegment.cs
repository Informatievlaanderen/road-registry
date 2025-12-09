namespace RoadRegistry.RoadSegment.Events.V1;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;

public class ImportedRoadSegment : IMartenEvent
{
    public required string AccessRestriction { get; set; }
    public required string Category { get; set; }
    public required int EndNodeId { get; set; }
    public required RoadSegmentGeometry Geometry { get; set; }
    public required string GeometryDrawMethod { get; set; }
    public required int GeometryVersion { get; set; }
    public required int Id { get; set; }
    public required ImportedRoadSegmentLaneAttribute[] Lanes { get; set; }
    public required ImportedRoadSegmentSideAttribute LeftSide { get; set; }
    public required MaintenanceAuthority MaintenanceAuthority { get; set; }
    public required string Morphology { get; set; }
    public required ImportedOriginProperties Origin { get; set; }
    public required ImportedRoadSegmentEuropeanRoadAttribute[] PartOfEuropeanRoads { get; set; }
    public required ImportedRoadSegmentNationalRoadAttribute[] PartOfNationalRoads { get; set; }
    public required ImportedRoadSegmentNumberedRoadAttribute[] PartOfNumberedRoads { get; set; }
    public required DateTime RecordingDate { get; set; }
    public required ImportedRoadSegmentSideAttribute RightSide { get; set; }
    public required int StartNodeId { get; set; }
    public required string Status { get; set; }
    public required ImportedRoadSegmentSurfaceAttribute[] Surfaces { get; set; }
    public required int Version { get; set; }
    public required DateTimeOffset When {get; set; }
    public required ImportedRoadSegmentWidthAttribute[] Widths { get; set; }

    public required ProvenanceData Provenance { get; set; }
}
