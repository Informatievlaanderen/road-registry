namespace RoadRegistry.RoadSegment.Events;

using System.Collections.Generic;
using BackOffice;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadNetwork.ValueObjects;
using ValueObjects;

public class RoadSegmentAdded: IHaveHash, ICreatedEvent
{
    public const string EventName = "RoadSegmentAdded";

    public required RoadSegmentId Id { get; init; }
    public required RoadSegmentId? OriginalId { get; init; }
    public required GeometryObject Geometry { get; init; }
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

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
