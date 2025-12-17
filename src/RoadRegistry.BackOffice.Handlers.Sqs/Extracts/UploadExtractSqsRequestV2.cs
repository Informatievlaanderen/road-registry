namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;

public sealed class UploadExtractSqsRequestV2 : SqsRequest
{
    public required DownloadId DownloadId { get; init; }
    public required UploadId UploadId { get; init; }
    public required ExtractRequestId ExtractRequestId { get; init; }
}
