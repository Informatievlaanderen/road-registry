namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RealizeRoadSegment;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

public sealed record RealizeRoadSegmentV2SqsLambdaRequest : SqsLambdaRequest
{
    public RealizeRoadSegmentV2SqsLambdaRequest(string groupId, RealizeRoadSegmentV2SqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public RealizeRoadSegmentV2SqsRequest Request { get; }
}
