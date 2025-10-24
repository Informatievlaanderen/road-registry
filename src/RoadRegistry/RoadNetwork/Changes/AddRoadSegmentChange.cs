namespace RoadRegistry.RoadNetwork.Changes;

using System.Collections.Generic;
using BackOffice;
using NetTopologySuite.Geometries;
using RoadSegment.ValueObjects;

public sealed record AddRoadSegmentChange : IRoadNetworkChange
{
    //public required RoadSegmentId Id { get; init; }
    public required RoadSegmentId TemporaryId { get; init; }
    public required RoadSegmentId? OriginalId { get; init; }
    public required RoadSegmentId? PermanentId { get; init; } //TODO-pr ik denk dat deze obsolete is nu dat er geen verhuis meer nodig is tussen streams
    public required RoadNodeId StartNodeId { get; init; }
    //public required RoadNodeId? TemporaryStartNodeId { get; init; }
    public required RoadNodeId EndNodeId { get; init; }
    //public required RoadNodeId? TemporaryEndNodeId { get; init; }
    public required MultiLineString Geometry { get; init; }
    public required RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; init; }
    public required RoadSegmentDynamicAttributeCollection<RoadSegmentAccessRestriction> AccessRestriction { get; init; }
    public required RoadSegmentDynamicAttributeCollection<RoadSegmentCategory> Category { get; init; }
    public required RoadSegmentDynamicAttributeCollection<RoadSegmentMorphology> Morphology { get; init; }
    public required RoadSegmentDynamicAttributeCollection<RoadSegmentStatus> Status { get; init; }
    public required RoadSegmentDynamicAttributeCollection<StreetNameLocalId> StreetNameId { get; init; }
    public required RoadSegmentDynamicAttributeCollection<OrganizationId> MaintenanceAuthorityId { get; init; }
    public required RoadSegmentDynamicAttributeCollection<RoadSegmentSurfaceType> SurfaceType { get; init; }
    public required IReadOnlyCollection<EuropeanRoadNumber> EuropeanRoadNumbers { get; init; }
    public required IReadOnlyCollection<NationalRoadNumber> NationalRoadNumbers { get; init; }
}
