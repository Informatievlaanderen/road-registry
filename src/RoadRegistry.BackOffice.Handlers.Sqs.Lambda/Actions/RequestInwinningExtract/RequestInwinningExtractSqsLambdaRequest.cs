namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RequestInwinningExtract;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

public sealed record RequestInwinningExtractSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<RequestInwinningExtractRequest>
{
    public RequestInwinningExtractSqsLambdaRequest(string groupId, RequestInwinningExtractSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest.Request;
    }

    public RequestInwinningExtractRequest Request { get; init; }
}
