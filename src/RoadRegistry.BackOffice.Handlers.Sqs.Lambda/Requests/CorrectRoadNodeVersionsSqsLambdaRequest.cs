namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests;

using Abstractions;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNodes;
using RoadSegments;

public sealed record CorrectRoadNodeVersionsSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<CorrectRoadNodeVersionsRequest>
{
    public CorrectRoadNodeVersionsSqsLambdaRequest(string groupId, CorrectRoadNodeVersionsSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest.Request;
    }

    public CorrectRoadNodeVersionsRequest Request { get; init; }
}
