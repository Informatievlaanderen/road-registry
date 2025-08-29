namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests.Extracts;

using Abstractions;
using Abstractions.Extracts.V2;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Sqs.Extracts;

public sealed record RequestExtractSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<RequestExtractRequest>
{
    public RequestExtractSqsLambdaRequest(string groupId, RequestExtractSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest.Request;
    }

    public RequestExtractRequest Request { get; init; }
}
