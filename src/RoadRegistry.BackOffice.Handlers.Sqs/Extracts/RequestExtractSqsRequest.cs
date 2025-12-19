namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Abstractions;
using Abstractions.Extracts.V2;
using NetTopologySuite.Geometries;

public sealed class RequestExtractSqsRequest : SqsRequest
{
    public required ExtractRequestId ExtractRequestId { get; init; }
    public required DownloadId DownloadId { get; init; }
    public required MultiPolygon Contour { get; init; }
    public required string Description { get; init; }
    public required bool IsInformative { get; init; }
    public required string? ExternalRequestId { get; init; }
}
