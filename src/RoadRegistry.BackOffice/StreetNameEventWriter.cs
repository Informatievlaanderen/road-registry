namespace RoadRegistry.BackOffice;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Messages;
using SqlStreamStore;
using System;
using System.Threading;
using System.Threading.Tasks;
using SqlStreamStore.Streams;

public interface IStreetNameEventWriter
{
    Task WriteAsync(StreetNameId id, object @event, CancellationToken cancellationToken);
}

public class StreetNameEventWriter : RoadRegistryEventWriter, IStreetNameEventWriter
{
    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(StreetNameEvents).Assembly));
    
    public StreetNameEventWriter(IStreamStore store, EventEnricher enricher)
        : base(store, enricher, EventMapping)
    {
    }
    
    public Task WriteAsync(StreetNameId id, object @event, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(StreetNameId.ToStreamName(id), Guid.NewGuid(), ExpectedVersion.Any, new []{ @event }, null, null, cancellationToken);
    }
}
