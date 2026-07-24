namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.SplitRoadSegmentsByJunction;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

public sealed record SplitRoadSegmentsByJunctionSqsLambdaRequest : SqsLambdaRequest
{
    public SplitRoadSegmentsByJunctionSqsLambdaRequest(string groupId, SplitRoadSegmentsByJunctionSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public SplitRoadSegmentsByJunctionSqsRequest Request { get; }
}
