namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record RequestInwinningExtractRequest(string ExtractRequestId, Guid DownloadId, string Contour, string NisCode) : EndpointRequest<RequestExtractResponse>
{
}
