namespace RoadRegistry.Extracts.ZipArchiveWriters;

using RoadRegistry.Extracts.Projections;

internal static class RoadSegmentFlattener
{
    internal static IReadOnlyCollection<FlatRoadSegment> Flatten(this RoadSegmentExtractItem roadSegment)
    {
        //TODO-pr flatten roadsegment according to all dynamic small segments
        throw new NotImplementedException();
    }
}

public sealed class FlatRoadSegment
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required RoadSegmentGeometry Geometry { get; init; }
    public required string GeometryDrawMethod { get; init; }
    public required string AccessRestriction { get; init; }
    public required string Category { get; init; }
    public required string Morphology { get; init; }
    public required string Status { get; init; }
    public required StreetNameLocalId LeftStreetNameId { get; init; }
    public required StreetNameLocalId RightStreetNameId { get; init; }
    public required OrganizationId LeftMaintenanceAuthorityId { get; init; }
    public required OrganizationId RightMaintenanceAuthorityId { get; init; }
    public required string SurfaceType { get; init; }
    public required List<EuropeanRoadNumber> EuropeanRoadNumbers { get; init; }
    public required List<NationalRoadNumber> NationalRoadNumbers { get; init; }

    public required EventTimestamp Origin { get; init; }
    public required EventTimestamp LastModified { get; init; }

    public required bool IsV2 { get; init; }
}
