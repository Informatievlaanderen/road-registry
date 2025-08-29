namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests.Extracts;

using Abstractions;
using Abstractions.Extracts.V2;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Sqs.Extracts;

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
