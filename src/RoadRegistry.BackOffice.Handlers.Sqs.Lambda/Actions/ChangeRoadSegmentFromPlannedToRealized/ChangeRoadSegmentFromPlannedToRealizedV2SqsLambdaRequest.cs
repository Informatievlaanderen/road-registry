namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentFromPlannedToRealized;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

public sealed record ChangeRoadSegmentFromPlannedToRealizedV2SqsLambdaRequest : SqsLambdaRequest
{
    public ChangeRoadSegmentFromPlannedToRealizedV2SqsLambdaRequest(string groupId, ChangeRoadSegmentFromPlannedToRealizedV2SqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public ChangeRoadSegmentFromPlannedToRealizedV2SqsRequest Request { get; }
}
