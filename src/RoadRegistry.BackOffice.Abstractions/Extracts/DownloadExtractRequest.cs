namespace RoadRegistry.BackOffice.Abstractions.Extracts;

using System.ComponentModel.DataAnnotations;

public sealed record DownloadExtractRequest(string RequestId, string Contour, bool IsInformative) : EndpointRequest<DownloadExtractResponse>
{
}
