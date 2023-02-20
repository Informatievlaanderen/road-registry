namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Requests;

using BackOffice.Abstractions;
using BackOffice.Abstractions.RoadNetworks;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadNetworks;

public sealed record RebuildRoadNetworkSnapshotSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<RebuildRoadNetworkSnapshotRequest>
{
    public RebuildRoadNetworkSnapshotSqsLambdaRequest(string groupId, RebuildRoadNetworkSnapshotSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest.Request;
    }

    public RebuildRoadNetworkSnapshotRequest Request { get; init; }
}
