namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.CorrectRoadSegmentFromRealizedToPlanned;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

public sealed record CorrectRoadSegmentFromRealizedToPlannedV2SqsLambdaRequest : SqsLambdaRequest
{
    public CorrectRoadSegmentFromRealizedToPlannedV2SqsLambdaRequest(string groupId, CorrectRoadSegmentFromRealizedToPlannedV2SqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public CorrectRoadSegmentFromRealizedToPlannedV2SqsRequest Request { get; }
}
