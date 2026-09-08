namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RemoveRoadNode;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNodes.V2;

public sealed record RemoveRoadNodeV2SqsLambdaRequest : SqsLambdaRequest
{
    public RemoveRoadNodeV2SqsLambdaRequest(string groupId, RemoveRoadNodeV2SqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public RemoveRoadNodeV2SqsRequest Request { get; }
}
