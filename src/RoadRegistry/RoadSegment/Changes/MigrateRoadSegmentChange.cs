namespace RoadRegistry.RoadSegment.Changes;

using System.Collections.Generic;
using ScopedRoadNetwork;
using ValueObjects;

public sealed record MigrateRoadSegmentChange : IRoadNetworkChange
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required RoadSegmentId? OriginalId { get; init; }
    public required RoadNodeId StartNodeId { get; init; }
    public required RoadNodeId EndNodeId { get; init; }
    public required RoadSegmentGeometry Geometry { get; init; }
    public required RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> AccessRestriction { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentCategory> Category { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> Morphology { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2> Status { get; init; }
    public required RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId { get; init; }
    public required RoadSegmentDynamicAttributeValues<OrganizationId> MaintenanceAuthorityId { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType> SurfaceType { get; init; }
    public required IReadOnlyCollection<EuropeanRoadNumber> EuropeanRoadNumbers { get; init; }
    public required IReadOnlyCollection<NationalRoadNumber> NationalRoadNumbers { get; init; }
}
