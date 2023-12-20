namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record DownloadFileContentRequest(string DownloadId, int DefaultRetryAfter, int RetryAfterAverageWindowInDays) : EndpointRequest<DownloadFileContentResponse>, IEndpointRetryableRequest
{
}
