namespace RoadRegistry.RoadSegment.Changes;

using ScopedRoadNetwork;
using ValueObjects;

public sealed record ModifyRoadSegmentChange : IRoadNetworkChange
{
    public required RoadSegmentIdReference RoadSegmentIdReference { get; init; }
    public RoadSegmentGeometry? Geometry { get; init; }
    public RoadSegmentGeometryDrawMethodV2? GeometryDrawMethod { get; init; }
    public RoadSegmentStatusV2? Status { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>? AccessRestriction { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>? Category { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>? Morphology { get; init; }
    public RoadSegmentDynamicAttributeValues<StreetNameLocalId>? StreetNameId { get; init; }
    public RoadSegmentDynamicAttributeValues<OrganizationId>? MaintenanceAuthorityId { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>? SurfaceType { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>? CarTrafficDirection { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>? BikeTrafficDirection { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection>? PedestrianTrafficDirection { get; init; }
}
