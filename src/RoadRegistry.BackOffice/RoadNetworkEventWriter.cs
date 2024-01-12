namespace RoadRegistry.BackOffice;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Messages;
using SqlStreamStore;
using SqlStreamStore.Streams;
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

    public Task WriteAsync(StreamName streamName, Guid messageId, object @event, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return AppendToStoreStream(streamName, messageId, ExpectedVersion.Any, new[] { @event }, null, null, cancellationToken);
    }

    public Task WriteAsync(StreamName streamName, Guid messageId, int expectedVersion, object[] events, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(streamName, messageId, expectedVersion, events, null, null, cancellationToken);
    }

    public Task WriteAsync(StreamName streamName, IRoadRegistryMessage message, object @event, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return AppendToStoreStream(streamName, message.MessageId, ExpectedVersion.Any, new[] { @event }, message.Principal, message.ProvenanceData, cancellationToken);
    }

    public Task WriteAsync(StreamName streamName, IRoadRegistryMessage message, int expectedVersion, object[] events, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(streamName, message.MessageId, expectedVersion, events, message.Principal, message.ProvenanceData, cancellationToken);
    }
}
