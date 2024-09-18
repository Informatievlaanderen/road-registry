namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record GetOverlappingTransactionZonesByNisCodeResponse : EndpointResponse
{
    public List<Guid> DownloadIds { get; init; }
}
