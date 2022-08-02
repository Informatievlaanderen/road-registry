namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record DownloadExtractRequest(string RequestId, string Contour) : EndpointRequest<DownloadExtractResponse>
{
}
