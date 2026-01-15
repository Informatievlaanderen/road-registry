namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;

public sealed class RequestInwinningExtractSqsRequest : SqsRequest
{
    public required ExtractRequestId ExtractRequestId { get; init; }
    public required DownloadId DownloadId { get; init; }
    public required ExtractGeometry Contour { get; init; }
    public required string NisCode { get; init; }
    public required string Description { get; init; }
    public required bool IsInformative { get; init; }
}
