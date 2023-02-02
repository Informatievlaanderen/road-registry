namespace RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;

using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using MediatR;
using NetTopologySuite.Geometries;

public sealed record CreateRoadSegmentOutlineRequest(MultiLineString Geometry, RoadSegmentStatus Status, RoadSegmentMorphology Morphology, RoadSegmentAccessRestriction AccessRestriction, OrganizationId MaintenanceAuthority, RoadSegmentSurfaceType SurfaceType, RoadSegmentWidth Width, RoadSegmentLaneCount LaneCount, RoadSegmentLaneDirection LaneDirection) : IRequest<ETagResponse>;
