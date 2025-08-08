namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Abstractions;
using Abstractions.Extracts.V2;

public sealed class UploadExtractSqsRequest : SqsRequest, IHasBackOfficeRequest<UploadExtractRequest>
{
    public UploadExtractRequest Request { get; init; }
    public string ExtractRequestId { get; init; }
}
