namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record GetOverlappingTransactionZonesByContourResponse : EndpointResponse
{
    public List<Guid> DownloadIds { get; init; }
}
