namespace RoadRegistry.BackOffice
{
    using System.Threading;
    using System.Threading.Tasks;
    using Framework;

    public interface IRoadNetworkExtractCommandQueue
    {
        Task Write(Command command, CancellationToken cancellationToken);
    }
}
