namespace RoadRegistry.BackOffice.Model
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRoadNetworkSnapshotWriter
    {
        Task WriteSnapshot(Messages.RoadNetworkSnapshot snapshot, int version, CancellationToken cancellationToken);
    }
}