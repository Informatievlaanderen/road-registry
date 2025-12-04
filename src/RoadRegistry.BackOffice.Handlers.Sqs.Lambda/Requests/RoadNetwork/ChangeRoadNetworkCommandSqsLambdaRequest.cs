namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests.RoadNetwork;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using CommandHandling.Actions.ChangeRoadNetwork;
using Sqs.RoadNetwork;

public sealed record ChangeRoadNetworkCommandSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<ChangeRoadNetworkCommand>
{
    public ChangeRoadNetworkCommandSqsLambdaRequest(string groupId, ChangeRoadNetworkCommandSqsRequest sqsRequest)
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
