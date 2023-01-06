namespace RoadRegistry.BackOffice;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Messages;
using SqlStreamStore;

public class RoadNetworkCommandQueue : RoadRegistryCommandQueue, IRoadNetworkCommandQueue
{
    private static readonly EventMapping CommandMapping =
        new(RoadNetworkCommands.All.ToDictionary(command => command.Name));

    public static readonly StreamName Stream = new("roadnetwork-command-queue");

    public RoadNetworkCommandQueue(IStreamStore store, ApplicationMetadata applicationMetadata)
        : base(store, CommandMapping, applicationMetadata)
    {
    }

    public Task Write(Command command, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(command, Stream, cancellationToken);
    }
}
