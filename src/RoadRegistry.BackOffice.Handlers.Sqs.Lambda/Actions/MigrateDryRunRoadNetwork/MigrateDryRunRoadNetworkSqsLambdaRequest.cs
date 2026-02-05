namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateDryRunRoadNetwork;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

public sealed record MigrateDryRunRoadNetworkSqsLambdaRequest : SqsLambdaRequest
{
    public MigrateDryRunRoadNetworkSqsLambdaRequest(string groupId, MigrateDryRunRoadNetworkSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public MigrateDryRunRoadNetworkSqsRequest Request { get; }
}
