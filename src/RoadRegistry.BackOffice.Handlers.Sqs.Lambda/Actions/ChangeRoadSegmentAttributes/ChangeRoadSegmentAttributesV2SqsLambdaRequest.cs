namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentAttributes;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

public sealed record ChangeRoadSegmentAttributesV2SqsLambdaRequest : SqsLambdaRequest
{
    public ChangeRoadSegmentAttributesV2SqsLambdaRequest(string groupId, ChangeRoadSegmentAttributesV2SqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public ChangeRoadSegmentAttributesV2SqsRequest Request { get; }
}
