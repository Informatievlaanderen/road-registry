namespace RoadRegistry.RoadNetwork.Changes;

using System.Collections.Generic;
using BackOffice;
using NetTopologySuite.Geometries;
using RoadSegment.ValueObjects;

public sealed record AddRoadSegmentChange : IRoadNetworkChange
{
    //public required RoadSegmentId Id { get; init; }
    public required RoadSegmentId TemporaryId { get; init; }
    public RoadSegmentId? OriginalId { get; init; }
    public RoadSegmentId? PermanentId { get; init; } //TODO-pr ik denk dat deze obsolete is nu dat er geen verhuis meer nodig is tussen streams
    public required RoadNodeId StartNodeId { get; init; }
    //public required RoadNodeId? TemporaryStartNodeId { get; init; }
    public required RoadNodeId EndNodeId { get; init; }
    //public required RoadNodeId? TemporaryEndNodeId { get; init; }
    public required MultiLineString Geometry { get; init; }
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
}
