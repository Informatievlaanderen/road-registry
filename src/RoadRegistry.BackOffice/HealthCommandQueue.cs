namespace RoadRegistry.BackOffice;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Messages;
using SqlStreamStore;

public interface IHealthCommandQueue
{
    Task WriteAsync(Command command, CancellationToken cancellationToken);
}

public class HealthCommandQueue : RoadRegistryCommandQueue, IHealthCommandQueue
{
    public static readonly EventMapping CommandMapping =
        new (HealthCommands.All.ToDictionary(command => command.Name));

    public static readonly StreamName Stream = new(WellKnownQueues.HealthCommandQueue);

    public HealthCommandQueue(IStreamStore store, ApplicationMetadata applicationMetadata)
        : base(store, CommandMapping, applicationMetadata)
    {
    }

    public Task WriteAsync(Command command, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(command, Stream, cancellationToken);
    }
}
