namespace RoadRegistry.RoadSegment.Changes;

using System.Collections.Generic;
using NetTopologySuite.Geometries;
using RoadNetwork;
using RoadRegistry.BackOffice;
using RoadRegistry.RoadSegment.ValueObjects;

public sealed record ModifyRoadSegmentChange : IRoadNetworkChange
{
    public required RoadSegmentId RoadSegmentId { get; init; }

    //public required RoadSegmentVersion Version { get; init; }
    //public required GeometryVersion? GeometryVersion { get; init; }
    public RoadSegmentId? OriginalId { get; init; }
    public MultiLineString? Geometry { get; init; }
    public RoadNodeId? StartNodeId { get; init; }
    //public required RoadNodeId? TemporaryStartNodeId { get; init; }
    public RoadNodeId? EndNodeId { get; init; }
    //public required RoadNodeId? TemporaryEndNodeId { get; init; }
    public RoadSegmentGeometryDrawMethod? GeometryDrawMethod { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>? AccessRestriction { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentCategory>? Category { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>? Morphology { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentStatus>? Status { get; init; }
    public RoadSegmentDynamicAttributeValues<StreetNameLocalId>? StreetNameId { get; init; }
    public RoadSegmentDynamicAttributeValues<OrganizationId>? MaintenanceAuthorityId { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>? SurfaceType { get; init; }
    public IReadOnlyCollection<EuropeanRoadNumber>? EuropeanRoadNumbers { get; init; }
    public IReadOnlyCollection<NationalRoadNumber>? NationalRoadNumbers { get; init; }

    //public required bool ConvertedFromOutlined { get; init; }
    public bool? CategoryModified { get; init; }
}
