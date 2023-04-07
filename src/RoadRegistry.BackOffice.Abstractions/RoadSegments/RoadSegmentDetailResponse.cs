namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using Messages;

public sealed record RoadSegmentDetailResponse(
    int RoadSegmentId,
    DateTime BeginTime,
    string? LastEventHash
) : EndpointResponse
{
    public RoadSegmentGeometry Geometry { get; set; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; set; }
    public int StartNodeId { get; set; }
    public int EndNodeId { get; set; }
    public int? LeftStreetNameId { get; set; }
    public string? LeftStreetName { get; set; }
    public int? RightStreetNameId { get; set; }
    public string? RightStreetName { get; set; }
    public RoadSegmentStatus Status { get; set; }
    public RoadSegmentMorphology Morphology { get; set; }
    public RoadSegmentAccessRestriction AccessRestriction { get; set; }
    public MaintenanceAuthority MaintenanceAuthority { get; set; }
    public RoadSegmentCategory Category { get; set; }
    public IReadOnlyCollection<RoadSegmentSurfaceTypeDetailResponse> SurfaceTypes { get; set; }
    public IReadOnlyCollection<RoadSegmentWidthDetailResponse> Widths { get; set; }
    public IReadOnlyCollection<RoadSegmentLaneCountDetailResponse> LaneCounts { get; set; }
}

public class RoadSegmentLaneCountDetailResponse
{
    public double FromPosition { get; set; }
    public double ToPosition { get; set; }
    public RoadSegmentLaneCount Count { get; set; }
    public RoadSegmentLaneDirection Direction { get; set; }
}

public class RoadSegmentWidthDetailResponse
{
    public double FromPosition { get; set; }
    public double ToPosition { get; set; }
    public RoadSegmentWidth Width { get; set; }
}

public class RoadSegmentSurfaceTypeDetailResponse
{
    public double FromPosition { get; set; }
    public double ToPosition { get; set; }
    public RoadSegmentSurfaceType SurfaceType { get; set; }
}
