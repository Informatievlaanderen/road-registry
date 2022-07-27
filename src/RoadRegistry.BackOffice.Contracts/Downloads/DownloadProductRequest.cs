namespace RoadRegistry.BackOffice.Contracts.Downloads;

public sealed record DownloadProductRequest(string Date) : EndpointRequest<DownloadProductResponse>
{
}
