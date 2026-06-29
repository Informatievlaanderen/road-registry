namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.CreateRoadSegmentOutline;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

public sealed record CreateRoadSegmentOutlineV2SqsLambdaRequest : SqsLambdaRequest
{
    public CreateRoadSegmentOutlineV2SqsLambdaRequest(string groupId, CreateRoadSegmentOutlineV2SqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public CreateRoadSegmentOutlineV2SqsRequest Request { get; init; }
}
