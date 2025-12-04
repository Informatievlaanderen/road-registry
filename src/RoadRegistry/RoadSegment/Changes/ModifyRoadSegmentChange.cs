namespace RoadRegistry.RoadSegment.Changes;

using System.Collections.Generic;
using NetTopologySuite.Geometries;
using RoadNetwork;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;

public sealed record ModifyRoadSegmentChange : IRoadNetworkChange
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public RoadSegmentId? OriginalId { get; init; }
    public MultiLineString? Geometry { get; init; }
    public RoadNodeId? StartNodeId { get; init; }
    public RoadNodeId? EndNodeId { get; init; }
    public RoadSegmentGeometryDrawMethod? GeometryDrawMethod { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>? AccessRestriction { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentCategory>? Category { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>? Morphology { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentStatus>? Status { get; init; }
    public RoadSegmentDynamicAttributeValues<StreetNameLocalId>? StreetNameId { get; init; }
    public RoadSegmentDynamicAttributeValues<OrganizationId>? MaintenanceAuthorityId { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>? SurfaceType { get; init; }
}
