namespace RoadRegistry.RoadSegment.Events.V2;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;

public record RoadSegmentMerged: IMartenEvent, ICreatedEvent
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required IReadOnlyCollection<RoadSegmentId> OriginalIds { get; init; }
    public required RoadSegmentGeometry Geometry { get; init; }
    public required RoadNodeId StartNodeId { get; init; }
    public required RoadNodeId EndNodeId { get; init; }
    public required RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction> AccessRestriction { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentCategory> Category { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentMorphology> Morphology { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentStatus> Status { get; init; }
    public required RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId { get; init; }
    public required RoadSegmentDynamicAttributeValues<OrganizationId> MaintenanceAuthorityId { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType> SurfaceType { get; init; }
    public required IReadOnlyCollection<EuropeanRoadNumber> EuropeanRoadNumbers { get; init; }
    public required IReadOnlyCollection<NationalRoadNumber> NationalRoadNumbers { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
