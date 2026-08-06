namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentGeometry;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

public sealed record ChangeRoadSegmentGeometryV2SqsLambdaRequest : SqsLambdaRequest
{
    public ChangeRoadSegmentGeometryV2SqsLambdaRequest(string groupId, ChangeRoadSegmentGeometryV2SqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public ChangeRoadSegmentGeometryV2SqsRequest Request { get; }
}
