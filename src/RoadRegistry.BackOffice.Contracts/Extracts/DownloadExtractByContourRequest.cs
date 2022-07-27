namespace RoadRegistry.BackOffice.Contracts.Extracts;

public sealed record DownloadExtractByContourRequest(string Contour, int Buffer, string Description) : EndpointRequest<DownloadExtractByContourResponse>
{
}
