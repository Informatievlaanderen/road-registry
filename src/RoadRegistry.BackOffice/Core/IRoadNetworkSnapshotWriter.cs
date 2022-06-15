namespace RoadRegistry.BackOffice.Core
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRoadNetworkSnapshotWriter
    {
        Task WriteSnapshot(Messages.RoadNetworkSnapshot snapshot, int version, CancellationToken cancellationToken);
        Task SetHeadToVersion(int version, CancellationToken cancellationToken);
    }
}
