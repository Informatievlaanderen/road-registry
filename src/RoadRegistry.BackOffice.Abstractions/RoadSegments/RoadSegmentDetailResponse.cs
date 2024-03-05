namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using Messages;

public sealed record RoadSegmentDetailResponse(
    int RoadSegmentId,
    DateTime BeginTime,
    int Version,
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
    public IReadOnlyCollection<RoadSegmentEuropeanRoadDetailResponse> EuropeanRoads { get; set; }
    public IReadOnlyCollection<RoadSegmentNationalRoadDetailResponse> NationalRoads { get; set; }
    public IReadOnlyCollection<RoadSegmentNumberedRoadDetailResponse> NumberedRoads { get; set; }
    public bool IsRemoved { get; set; }
}

public sealed record RoadSegmentLaneCountDetailResponse
{
    public double FromPosition { get; init; }
    public double ToPosition { get; init; }
    public RoadSegmentLaneCount Count { get; init; }
    public RoadSegmentLaneDirection Direction { get; init; }
}

public sealed record RoadSegmentWidthDetailResponse
{
    public double FromPosition { get; init; }
    public double ToPosition { get; init; }
    public RoadSegmentWidth Width { get; init; }
}

public sealed record RoadSegmentSurfaceTypeDetailResponse
{
    public double FromPosition { get; init; }
    public double ToPosition { get; init; }
    public RoadSegmentSurfaceType SurfaceType { get; init; }
}

public sealed record RoadSegmentEuropeanRoadDetailResponse
{
    public EuropeanRoadNumber Number { get; init; }
}

public sealed record RoadSegmentNationalRoadDetailResponse
{
    public NationalRoadNumber Number { get; init; }
}

public sealed record RoadSegmentNumberedRoadDetailResponse
{
    public NumberedRoadNumber Number { get; init; }
    public RoadSegmentNumberedRoadDirection Direction { get; init; }
    public RoadSegmentNumberedRoadOrdinal Ordinal { get; init; }
}
