namespace RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;

using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using MediatR;
using Messages;

public sealed record CreateRoadSegmentOutlineRequest(
    RoadSegmentGeometry Geometry,
    RoadSegmentStatus Status,
    RoadSegmentMorphology Morphology,
    RoadSegmentAccessRestriction AccessRestriction,
    OrganizationId MaintenanceAuthority,
    RoadSegmentSurfaceType SurfaceType,
    RoadSegmentWidth Width,
    RoadSegmentLaneCount LaneCount,
    RoadSegmentLaneDirection LaneDirection,
    RoadSegmentCategory Category) : IRequest<ETagResponse>;
