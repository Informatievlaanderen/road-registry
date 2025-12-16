namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.CloseExtract;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

public sealed record CloseExtractSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<CloseExtractRequest>
{
    public CloseExtractSqsLambdaRequest(string groupId, CloseExtractSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest.Request;
    }

    public CloseExtractRequest Request { get; init; }
}
