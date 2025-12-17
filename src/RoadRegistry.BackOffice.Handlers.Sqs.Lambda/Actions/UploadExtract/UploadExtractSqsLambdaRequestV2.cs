namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.UploadExtract;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

public sealed record UploadExtractSqsLambdaRequestV2 : SqsLambdaRequest
{
    public UploadExtractSqsLambdaRequestV2(string groupId, UploadExtractSqsRequestV2 sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public UploadExtractSqsRequestV2 Request { get; }
}
