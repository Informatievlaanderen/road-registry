namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record UploadStatusRequest(string UploadId, int DefaultRetryAfter, int RetryAfterAverageWindowInDays) : EndpointRequest<UploadStatusResponse>, IEndpointRetryableRequest
{
}
