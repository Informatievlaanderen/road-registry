namespace RoadRegistry.Snapshot.Handlers.Sqs.RoadNetworks;

using BackOffice.Abstractions;
using BackOffice.Abstractions.RoadNetworks;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;

public class RebuildRoadNetworkSnapshotSqsRequest : SqsRequest, IHasBackOfficeRequest<RebuildRoadNetworkSnapshotRequest>
{
    public required RebuildRoadNetworkSnapshotRequest Request { get; init; }
}
