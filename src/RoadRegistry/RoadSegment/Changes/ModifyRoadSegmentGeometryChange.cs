namespace RoadRegistry.RoadSegment.Changes;

using ValueObjects;

// 'Wijzig de geometrie van een wegsegment'. The new geometry always comes with the full set of dynamically segmented
// attributes: changing the geometry changes the segment's length, so the positions the attribute values apply to
// change with it and cannot be inferred. Status and geometry draw method are never touched by this action.
//
// Deliberately not an IRoadNetworkChange: like a split, this operation performs its own very specific mutations
// (dragging road nodes and the segments hanging off them) rather than being fed through the generic change pipeline.
public sealed record ModifyRoadSegmentGeometryChange
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required RoadSegmentGeometry Geometry { get; init; }
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
