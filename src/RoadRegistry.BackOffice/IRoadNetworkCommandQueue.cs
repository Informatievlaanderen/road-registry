namespace RoadRegistry.BackOffice
{
    using System.Threading;
    using System.Threading.Tasks;
    using Framework;
    using Messages;

    public interface IRoadNetworkCommandQueue
    {
        Task Write(Command command, CancellationToken cancellationToken);
    }
}
