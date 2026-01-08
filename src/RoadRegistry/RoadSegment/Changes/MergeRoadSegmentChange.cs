namespace RoadRegistry.RoadSegment.Changes;

using System.Collections.Generic;
using NetTopologySuite.Geometries;
using ScopedRoadNetwork;
using ValueObjects;

public sealed record MergeRoadSegmentChange : IRoadNetworkChange
{
    public required RoadSegmentId TemporaryId { get; init; }
    public required IReadOnlyCollection<RoadSegmentId> OriginalIds { get; init; }
    public required RoadNodeId StartNodeId { get; init; }
    public required RoadNodeId EndNodeId { get; init; }
    public required MultiLineString Geometry { get; init; }
    public required RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction> AccessRestriction { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentCategory> Category { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentMorphology> Morphology { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentStatus> Status { get; init; }
    public required RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId { get; init; }
    public required RoadSegmentDynamicAttributeValues<OrganizationId> MaintenanceAuthorityId { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType> SurfaceType { get; init; }
    public IReadOnlyCollection<EuropeanRoadNumber> EuropeanRoadNumbers { get; init; } = [];
    public IReadOnlyCollection<NationalRoadNumber> NationalRoadNumbers { get; init; } = [];
}
