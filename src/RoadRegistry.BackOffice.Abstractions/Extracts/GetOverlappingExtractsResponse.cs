namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record GetOverlappingExtractsResponse : EndpointResponse
{
    public List<Guid> DownloadIds { get; init; }
}
