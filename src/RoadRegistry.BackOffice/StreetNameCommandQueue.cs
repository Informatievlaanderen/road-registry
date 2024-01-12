namespace RoadRegistry.BackOffice;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Messages;
using SqlStreamStore;

public class StreetNameCommandQueue : RoadRegistryCommandQueue
{
    private static readonly EventMapping CommandMapping =
        new(StreetNameEvents.All.ToDictionary(command => command.Name));

    private static readonly StreamName StreamNamePrefix = new("streetname-");

    public StreetNameCommandQueue(IStreamStore store)
        : base(store, CommandMapping)
    {
    }

    private StreamName GetStreamName(StreetNameId id)
    {
        return new StreamName($"{StreamNamePrefix}{id}");
    }

    public Task Write(StreetNameId id, Command command, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(command, GetStreamName(id), cancellationToken);
    }
}
