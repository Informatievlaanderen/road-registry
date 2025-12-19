namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RemoveRoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadNetwork;

public sealed record RemoveRoadSegmentsSqsLambdaRequest : SqsLambdaRequest
{
    public RemoveRoadSegmentsSqsLambdaRequest(string groupId, RemoveRoadSegmentsSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public RemoveRoadSegmentsSqsRequest Request { get; }
}
