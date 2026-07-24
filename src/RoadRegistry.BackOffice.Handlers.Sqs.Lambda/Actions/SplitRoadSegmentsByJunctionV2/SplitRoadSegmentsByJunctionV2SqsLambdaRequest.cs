namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.SplitRoadSegmentsByJunctionV2;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

public sealed record SplitRoadSegmentsByJunctionV2SqsLambdaRequest : SqsLambdaRequest
{
    public SplitRoadSegmentsByJunctionV2SqsLambdaRequest(string groupId, SplitRoadSegmentsByJunctionV2SqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public SplitRoadSegmentsByJunctionV2SqsRequest Request { get; }
}
