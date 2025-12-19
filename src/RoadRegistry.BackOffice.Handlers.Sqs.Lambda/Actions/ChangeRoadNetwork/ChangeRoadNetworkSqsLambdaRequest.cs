namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadNetwork;

public sealed record ChangeRoadNetworkSqsLambdaRequest : SqsLambdaRequest
{
    public ChangeRoadNetworkSqsLambdaRequest(string groupId, ChangeRoadNetworkSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public ChangeRoadNetworkSqsRequest Request { get; }
}
