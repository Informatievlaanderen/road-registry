namespace RoadRegistry.BackOffice;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Messages;
using SqlStreamStore;
using System;
using System.Threading;
using System.Threading.Tasks;

public interface IRoadNetworkEventWriter
{
    Task WriteAsync(StreamName streamName, int expectedVersion, Event @event, CancellationToken cancellationToken);
    Task WriteAsync(StreamName streamName, int expectedVersion, IRoadRegistryMessage message, object[] events, CancellationToken cancellationToken);
}

public class RoadNetworkEventWriter : RoadRegistryEventWriter, IRoadNetworkEventWriter
{
    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    public RoadNetworkEventWriter(IStreamStore store, EventEnricher enricher)
        : base(store, enricher, EventMapping)
    {
    }
    
    public Task WriteAsync(StreamName streamName, int expectedVersion, Event @event, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return AppendToStoreStream(streamName, @event.MessageId, expectedVersion, new[] { @event.Body }, null, null, cancellationToken);
    }
    
    public Task WriteAsync(StreamName streamName, int expectedVersion, IRoadRegistryMessage message, object[] events, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(streamName, message.MessageId, expectedVersion, events, message.Principal, message.ProvenanceData, cancellationToken);
    }
}
