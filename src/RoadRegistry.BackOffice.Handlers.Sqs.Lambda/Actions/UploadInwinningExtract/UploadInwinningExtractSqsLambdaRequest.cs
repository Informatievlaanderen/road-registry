namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.UploadInwinningExtract;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

public sealed record UploadInwinningExtractSqsLambdaRequest : SqsLambdaRequest
{
    public UploadInwinningExtractSqsLambdaRequest(string groupId, UploadInwinningExtractSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public UploadInwinningExtractSqsRequest Request { get; }
}
