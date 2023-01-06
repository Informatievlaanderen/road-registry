namespace RoadRegistry.BackOffice;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Messages;
using SqlStreamStore;

public class RoadNetworkExtractCommandQueue : RoadRegistryCommandQueue, IRoadNetworkExtractCommandQueue
{
    private static readonly EventMapping CommandMapping =
        new(RoadNetworkCommands.All.ToDictionary(command => command.Name));

    public static readonly StreamName Stream = new("roadnetworkextract-command-queue");

    public RoadNetworkExtractCommandQueue(IStreamStore store)
        : base(store, CommandMapping)
    {
    }

    public Task Write(Command command, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(command, Stream, cancellationToken);
    }
}
