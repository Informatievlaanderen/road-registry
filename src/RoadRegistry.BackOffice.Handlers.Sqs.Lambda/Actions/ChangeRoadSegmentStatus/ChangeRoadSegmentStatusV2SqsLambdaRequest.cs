namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentStatus;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

public sealed record ChangeRoadSegmentStatusV2SqsLambdaRequest : SqsLambdaRequest
{
    public ChangeRoadSegmentStatusV2SqsLambdaRequest(string groupId, ChangeRoadSegmentStatusV2SqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public ChangeRoadSegmentStatusV2SqsRequest Request { get; }
}
