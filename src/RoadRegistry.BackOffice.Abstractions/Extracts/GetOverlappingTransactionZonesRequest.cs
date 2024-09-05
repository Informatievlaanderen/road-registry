namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record GetOverlappingTransactionZonesRequest : EndpointRequest<GetOverlappingTransactionZonesResponse>
{
    public string? NisCode { get; init; }
    public int Buffer { get; init; }
    public string? Contour { get; init; }
}
