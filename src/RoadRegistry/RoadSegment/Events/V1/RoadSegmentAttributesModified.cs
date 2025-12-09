namespace RoadRegistry.RoadSegment.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;

public class RoadSegmentAttributesModified : IMartenEvent
{
    public required int Id { get; set; }
    public required int Version { get; set; }
    public required MaintenanceAuthority? MaintenanceAuthority { get; set; }
    public required string? Status { get; set; }
    public required string? Morphology { get; set; }
    public required string? AccessRestriction { get; set; }
    public required string? Category { get; set; }
    public required RoadSegmentSideAttributes? LeftSide { get; set; }
    public required RoadSegmentSideAttributes? RightSide { get; set; }
    public required RoadSegmentLaneAttributes[]? Lanes { get; set; }
    public required RoadSegmentSurfaceAttributes[]? Surfaces { get; set; }
    public required RoadSegmentWidthAttributes[]? Widths { get; set; }

    public required ProvenanceData Provenance { get; set; }
}
