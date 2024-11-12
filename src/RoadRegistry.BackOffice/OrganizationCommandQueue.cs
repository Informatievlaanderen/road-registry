namespace RoadRegistry.BackOffice;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Messages;
using SqlStreamStore;

public interface IOrganizationCommandQueue
{
    Task WriteAsync(Command command, CancellationToken cancellationToken);
}

public class OrganizationCommandQueue : RoadRegistryCommandQueue, IOrganizationCommandQueue
{
    public static readonly EventMapping CommandMapping =
        new (OrganizationCommands.All.ToDictionary(command => command.Name));

    public static readonly StreamName Stream = new(WellKnownQueues.OrganizationCommandQueue);

    public OrganizationCommandQueue(IStreamStore store, ApplicationMetadata applicationMetadata)
        : base(store, CommandMapping, applicationMetadata)
    {
    }

    public Task WriteAsync(Command command, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(command, Stream, cancellationToken);
    }
}
