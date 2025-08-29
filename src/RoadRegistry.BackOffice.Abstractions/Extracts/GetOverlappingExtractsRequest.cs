namespace RoadRegistry.BackOffice.Abstractions.Extracts;

using NetTopologySuite.Geometries;

public sealed record GetOverlappingExtractsRequest : EndpointRequest<GetOverlappingExtractsResponse>
{
    public Geometry Contour { get; init; }
}
