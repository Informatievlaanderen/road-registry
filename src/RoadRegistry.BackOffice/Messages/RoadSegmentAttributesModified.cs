namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class RoadSegmentAttributesModified : IHaveHash
{
    public const string EventName = "RoadSegmentAttributesModified";

    public int Id { get; set; }
    public int Version { get; set; }

    public MaintenanceAuthority? MaintenanceAuthority { get; set; }
    public string? Status { get; set; }
    public string? Morphology { get; set; }
    public string? AccessRestriction { get; set; }
    public string? Category { get; set; }
    public RoadSegmentSideAttributes? LeftSide { get; set; }
    public RoadSegmentSideAttributes? RightSide { get; set; }
    public RoadSegmentLaneAttributes[]? Lanes { get; set; }
    public RoadSegmentSurfaceAttributes[]? Surfaces { get; set; }
    public RoadSegmentWidthAttributes[]? Widths { get; set; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
