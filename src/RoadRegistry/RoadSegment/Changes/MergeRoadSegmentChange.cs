namespace RoadRegistry.RoadSegment.Changes;

using System.Collections.Generic;
using ScopedRoadNetwork;
using ValueObjects;

public sealed record MergeRoadSegmentChange : IRoadNetworkChange
{
    public required RoadSegmentId TemporaryId { get; init; }
    public required IReadOnlyCollection<RoadSegmentId> OriginalIds { get; init; }
    public required RoadSegmentGeometry Geometry { get; init; }
    public required RoadSegmentGeometryDrawMethodV2 GeometryDrawMethod { get; init; }
    public required RoadSegmentStatusV2 Status { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> AccessRestriction { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2> Category { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> Morphology { get; init; }
    public required RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId { get; init; }
    public required RoadSegmentDynamicAttributeValues<OrganizationId> MaintenanceAuthorityId { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> SurfaceType { get; init; }
    public required RoadSegmentDynamicAttributeValues<bool> CarAccessForward { get; init; }
    public required RoadSegmentDynamicAttributeValues<bool> CarAccessBackward { get; init; }
    public required RoadSegmentDynamicAttributeValues<bool> BikeAccessForward { get; init; }
    public required RoadSegmentDynamicAttributeValues<bool> BikeAccessBackward { get; init; }
    public required RoadSegmentDynamicAttributeValues<bool> PedestrianAccess { get; init; }
    public IReadOnlyCollection<EuropeanRoadNumber> EuropeanRoadNumbers { get; init; } = [];
    public IReadOnlyCollection<NationalRoadNumber> NationalRoadNumbers { get; init; } = [];
}
