namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record RequestExtractRequest(string ExtractRequestId, Guid DownloadId, string Contour, string Description, bool IsInformative, string? ExternalRequestId) : EndpointRequest<RequestExtractResponse>
{
}
