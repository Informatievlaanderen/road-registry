namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record ExtractDetailsRequest(DownloadId DownloadId, int DefaultRetryAfter, int RetryAfterAverageWindowInDays) : EndpointRequest<ExtractDetailsResponse>, IEndpointRetryableRequest
{
}
