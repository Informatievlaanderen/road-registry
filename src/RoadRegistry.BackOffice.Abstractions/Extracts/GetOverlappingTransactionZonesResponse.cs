namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record GetOverlappingTransactionZonesResponse : EndpointResponse
{
    public List<Guid> DownloadIds { get; init; }
}
