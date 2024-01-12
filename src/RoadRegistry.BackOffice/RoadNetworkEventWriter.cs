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
    Task WriteAsync(StreamName streamName, Guid messageId, object @event, CancellationToken cancellationToken);
    Task WriteAsync(StreamName streamName, Guid messageId, int version, object[] events, CancellationToken cancellationToken);
    Task WriteAsync(StreamName streamName, IRoadRegistryMessage message, object @event, CancellationToken cancellationToken);
    Task WriteAsync(StreamName streamName, IRoadRegistryMessage message, int expectedVersion, object[] events, CancellationToken cancellationToken);
}

public class RoadNetworkEventWriter : RoadRegistryEventWriter, IRoadNetworkEventWriter
{
    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    public RoadNetworkEventWriter(IStreamStore store, EventEnricher enricher)
        : base(store, enricher, EventMapping)
    {
    }

    public static IRoadNetworkEventWriter Create(IStreamStore store, EventEnricher enricher)
    {
        return new RoadNetworkEventWriter(store, enricher);
    }
}
