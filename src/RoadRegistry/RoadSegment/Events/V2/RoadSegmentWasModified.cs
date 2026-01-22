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
    public RoadSegmentGeometryDrawMethod? GeometryDrawMethod { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>? AccessRestriction { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentCategory>? Category { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>? Morphology { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>? Status { get; init; }
    public RoadSegmentDynamicAttributeValues<StreetNameLocalId>? StreetNameId { get; init; }
    public RoadSegmentDynamicAttributeValues<OrganizationId>? MaintenanceAuthorityId { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>? SurfaceType { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
