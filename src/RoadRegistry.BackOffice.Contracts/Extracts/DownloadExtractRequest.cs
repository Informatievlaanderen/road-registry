namespace RoadRegistry.BackOffice.Contracts.Extracts;

public sealed record DownloadExtractRequest(string RequestId, string Contour) : EndpointRequest<DownloadExtractResponse>
{
}
