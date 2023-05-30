namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record DownloadExtractRequest(string RequestId, string Contour, bool UploadExpected = true) : EndpointRequest<DownloadExtractResponse>
{
}
