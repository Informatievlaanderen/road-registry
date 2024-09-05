namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record GetOverlappingTransactionZonesByNisCodeRequest : EndpointRequest<GetOverlappingTransactionZonesByNisCodeResponse>
{
    public string NisCode { get; init; }
    public int Buffer { get; init; }
}
