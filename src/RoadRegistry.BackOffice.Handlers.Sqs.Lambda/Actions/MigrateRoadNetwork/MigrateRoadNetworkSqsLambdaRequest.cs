namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateRoadNetwork;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

public sealed record MigrateRoadNetworkSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<ChangeRoadNetworkCommand>
{
    public MigrateRoadNetworkSqsLambdaRequest(string groupId, MigrateRoadNetworkSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest.Request;
    }

    public ChangeRoadNetworkCommand Request { get; init; }
}
