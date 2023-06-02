namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record DownloadExtractByNisCodeResponse(DownloadId DownloadId, bool IsInformative) : EndpointResponse
{
}
