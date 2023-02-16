namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Requests;

using BackOffice.Abstractions;
using BackOffice.Abstractions.RoadNetworks;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadNetworks;

public sealed record CreateRoadNetworkSnapshotSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<CreateRoadNetworkSnapshotRequest>
{
    public CreateRoadNetworkSnapshotSqsLambdaRequest(string groupId, CreateRoadNetworkSnapshotSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest.Request;
    }

    public CreateRoadNetworkSnapshotRequest Request { get; init; }
}
