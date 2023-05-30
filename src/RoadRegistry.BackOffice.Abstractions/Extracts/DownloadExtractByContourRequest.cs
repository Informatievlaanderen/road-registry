namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record DownloadExtractByContourRequest(string Contour, int Buffer, string Description, bool UploadExpected = true) : EndpointRequest<DownloadExtractByContourResponse>
{
}
