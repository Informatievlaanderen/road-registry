namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record GetDownloadFilePreSignedUrlRequest(string DownloadId, int DefaultRetryAfter, int RetryAfterAverageWindowInDays) : EndpointRequest<GetDownloadFilePreSignedUrlResponse>, IEndpointRetryableRequest
{
}
