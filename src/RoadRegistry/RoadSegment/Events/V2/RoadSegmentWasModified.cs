namespace RoadRegistry.RoadSegment.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;

public record RoadSegmentWasModified : IMartenEvent
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public RoadSegmentId? OriginalId { get; init; }
    public RoadSegmentGeometry? Geometry { get; init; }
    public RoadNodeId? StartNodeId { get; init; }
    public RoadNodeId? EndNodeId { get; init; }
    public RoadSegmentGeometryDrawMethodV2? GeometryDrawMethod { get; init; }
    public RoadSegmentStatusV2? Status { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>? AccessRestriction { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>? Category { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>? Morphology { get; init; }
    public RoadSegmentDynamicAttributeValues<StreetNameLocalId>? StreetNameId { get; init; }
    public RoadSegmentDynamicAttributeValues<OrganizationId>? MaintenanceAuthorityId { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>? SurfaceType { get; init; }
    public required RoadSegmentDynamicAttributeValues<bool>? CarAccessForward { get; init; }
    public required RoadSegmentDynamicAttributeValues<bool>? CarAccessBackward { get; init; }
    public required RoadSegmentDynamicAttributeValues<bool>? BikeAccessForward { get; init; }
    public required RoadSegmentDynamicAttributeValues<bool>? BikeAccessBackward { get; init; }
    public required RoadSegmentDynamicAttributeValues<bool>? PedestrianAccess { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
