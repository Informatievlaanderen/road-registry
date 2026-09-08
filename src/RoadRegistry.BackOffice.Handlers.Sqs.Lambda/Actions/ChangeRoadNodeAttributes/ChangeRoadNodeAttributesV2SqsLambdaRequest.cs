namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNodeAttributes;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNodes.V2;

public sealed record ChangeRoadNodeAttributesV2SqsLambdaRequest : SqsLambdaRequest
{
    public ChangeRoadNodeAttributesV2SqsLambdaRequest(string groupId, ChangeRoadNodeAttributesV2SqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public ChangeRoadNodeAttributesV2SqsRequest Request { get; }
}
