namespace RoadRegistry.BackOffice.Abstractions.Downloads;

public sealed record DownloadProductRequest(string Date) : EndpointRequest<DownloadProductResponse>
{
}