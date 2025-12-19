namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using NetTopologySuite.Geometries;

public sealed class RequestInwinningExtractSqsRequest : SqsRequest
{
    public required ExtractRequestId ExtractRequestId { get; init; }
    public required DownloadId DownloadId { get; init; }
    public required MultiPolygon Contour { get; init; }
    public required string NisCode { get; init; }
}
