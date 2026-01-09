namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateRoadNetwork;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

public sealed record MigrateRoadNetworkSqsLambdaRequest : SqsLambdaRequest
{
    public MigrateRoadNetworkSqsLambdaRequest(string groupId, MigrateRoadNetworkSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public MigrateRoadNetworkSqsRequest Request { get; }
}
