namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests.Extracts;

using Abstractions;
using Abstractions.Extracts.V2;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Sqs.Extracts;

public sealed record UploadExtractSqsLambdaRequestV2 :
    SqsLambdaRequest,
    IHasBackOfficeRequest<UploadExtractRequest>
{
    public UploadExtractSqsLambdaRequestV2(string groupId, UploadExtractSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest.Request;
    }

    public UploadExtractRequest Request { get; init; }
}
