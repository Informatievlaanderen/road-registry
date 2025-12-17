namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Abstractions;
using Abstractions.Extracts.V2;

public sealed class CloseExtractSqsRequest : SqsRequest
{
    public required DownloadId DownloadId { get; init; }
}
