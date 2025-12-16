namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.UploadExtract;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

public sealed record UploadExtractSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<UploadExtractRequest>
{
    public UploadExtractSqsLambdaRequest(string groupId, UploadExtractSqsRequest sqsRequest)
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
