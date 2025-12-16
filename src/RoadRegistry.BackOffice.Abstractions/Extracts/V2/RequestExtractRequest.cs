namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

using NetTopologySuite.Geometries;

public sealed record RequestExtractRequest(string ExtractRequestId, Guid DownloadId, MultiPolygon Contour, string Description, bool IsInformative, string? ExternalRequestId) : EndpointRequest<RequestExtractResponse>
{
}
