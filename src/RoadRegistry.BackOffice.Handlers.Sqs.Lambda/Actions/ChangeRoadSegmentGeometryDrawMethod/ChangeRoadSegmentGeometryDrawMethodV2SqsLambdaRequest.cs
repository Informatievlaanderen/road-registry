namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentGeometryDrawMethod;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

public sealed record ChangeRoadSegmentGeometryDrawMethodV2SqsLambdaRequest : SqsLambdaRequest
{
    public ChangeRoadSegmentGeometryDrawMethodV2SqsLambdaRequest(string groupId, ChangeRoadSegmentGeometryDrawMethodV2SqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public ChangeRoadSegmentGeometryDrawMethodV2SqsRequest Request { get; }
}
