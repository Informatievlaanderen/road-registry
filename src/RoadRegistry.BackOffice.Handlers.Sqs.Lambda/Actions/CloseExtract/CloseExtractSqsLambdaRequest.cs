namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.CloseExtract;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

public sealed record CloseExtractSqsLambdaRequest : SqsLambdaRequest
{
    public CloseExtractSqsLambdaRequest(string groupId, CloseExtractSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public CloseExtractSqsRequest Request { get; }
}
