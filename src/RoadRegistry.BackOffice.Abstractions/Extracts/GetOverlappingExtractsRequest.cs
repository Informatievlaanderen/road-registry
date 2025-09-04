namespace RoadRegistry.BackOffice.Abstractions.Extracts;

using NetTopologySuite.Geometries;

public sealed record GetOverlappingExtractsRequest(Geometry Contour) : EndpointRequest<GetOverlappingExtractsResponse>;
