namespace RoadRegistry.RoadNetwork.Changes;

using System.Collections.Generic;
using BackOffice;
using BackOffice.Core;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadNetwork;
using RoadSegment.ValueObjects;
using ValueObjects;

public sealed record ModifyRoadSegmentChange : IRoadNetworkChange
{
    public required RoadSegmentId Id { get; init; }

    //public required RoadSegmentVersion Version { get; init; }
    //public required GeometryVersion? GeometryVersion { get; init; }
    public required RoadSegmentId? OriginalId { get; init; }
    public required MultiLineString? Geometry { get; init; }
    public required RoadNodeId? StartNodeId { get; init; }
    //public required RoadNodeId? TemporaryStartNodeId { get; init; }
    public required RoadNodeId? EndNodeId { get; init; }
    //public required RoadNodeId? TemporaryEndNodeId { get; init; }
    public required RoadSegmentAccessRestriction? AccessRestriction { get; init; }
    public required RoadSegmentCategory? Category { get; init; }
    public required RoadSegmentGeometryDrawMethod? GeometryDrawMethod { get; init; }
    public required OrganizationId? MaintenanceAuthorityId { get; init; }
    public required RoadSegmentMorphology? Morphology { get; init; }
    public required RoadSegmentStatus? Status { get; init; }
    public required StreetNameLocalId? LeftSideStreetNameId { get; init; }
    public required StreetNameLocalId? RightSideStreetNameId { get; init; }
    public required IReadOnlyList<RoadSegmentLaneAttribute>? Lanes { get; init; }
    public required IReadOnlyList<RoadSegmentSurfaceAttribute>? Surfaces { get; init; }
    public required IReadOnlyList<RoadSegmentWidthAttribute>? Widths { get; init; }

    public required bool ConvertedFromOutlined { get; init; }
    public required bool? CategoryModified { get; init; }
}
