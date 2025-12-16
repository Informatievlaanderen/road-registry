namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

using NetTopologySuite.Geometries;

public sealed record RequestInwinningExtractRequest(string ExtractRequestId, Guid DownloadId, MultiPolygon Contour, string NisCode) : EndpointRequest<RequestExtractResponse>
{
}
