namespace RoadRegistry.RoadSegment.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public class RoadSegmentAttributesModified : IMartenEvent
{
    public const string EventName = "RoadSegmentAttributesModified"; // BE CAREFUL CHANGING THIS!!

    public required int RoadSegmentId { get; set; }
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

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
