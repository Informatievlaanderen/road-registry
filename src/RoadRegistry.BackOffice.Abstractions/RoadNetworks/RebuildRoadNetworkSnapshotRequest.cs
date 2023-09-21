namespace RoadRegistry.BackOffice.Abstractions.RoadNetworks
{
    using MediatR;

    public class RebuildRoadNetworkSnapshotRequest: IRequest<RebuildRoadNetworkSnapshotResponse>
    {
        public int MaxStreamVersion { get; set; }
    }
}
