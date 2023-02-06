namespace RoadRegistry.Snapshot.Handlers.Sqs.RoadNetworks;

using BackOffice.Abstractions;
using BackOffice.Abstractions.RoadNetworks;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;

public class CreateRoadNetworkSnapshotSqsRequest : SqsRequest, IHasBackOfficeRequest<CreateRoadNetworkSnapshotRequest>
{
    public CreateRoadNetworkSnapshotRequest Request { get; init; }
}