namespace RoadRegistry.BackOffice;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Messages;
using SqlStreamStore;
using SqlStreamStore.Streams;
using System.Threading;
using System.Threading.Tasks;

public interface IStreetNameEventWriter
{
    Task WriteAsync(Event @event, CancellationToken cancellationToken);
    Task WriteAsync(StreetNameLocalId id, Event @event, CancellationToken cancellationToken);
}

public class StreetNameEventWriter : RoadRegistryEventWriter, IStreetNameEventWriter
{
    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));
    
    public StreetNameEventWriter(IStreamStore store, EventEnricher enricher)
        : base(store, enricher, EventMapping)
    {
    }
    
    public Task WriteAsync(StreetNameLocalId id, Event @event, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(StreetNameLocalId.ToStreamName(id), @event.MessageId, ExpectedVersion.Any, new [] { @event.Body }, null, null, cancellationToken);
    }
    
    public Task WriteAsync(Event @event, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(new StreamName("streetnames"), @event.MessageId, ExpectedVersion.Any, new [] { @event.Body }, null, null, cancellationToken);
    }
}
