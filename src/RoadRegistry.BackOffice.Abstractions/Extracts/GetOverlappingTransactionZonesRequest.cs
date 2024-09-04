namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record GetOverlappingTransactionZonesRequest(string NisCode, string Contour) : EndpointRequest<GetOverlappingTransactionZonesResponse>
{
}
