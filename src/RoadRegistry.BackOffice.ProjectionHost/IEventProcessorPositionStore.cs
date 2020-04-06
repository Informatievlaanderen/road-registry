namespace RoadRegistry.BackOffice.ProjectionHost
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IEventProcessorPositionStore
    {
        Task<long?> ReadPosition(string name, CancellationToken cancellationToken);

        Task WritePosition(string name, long position, CancellationToken cancellationToken);
    }
}
