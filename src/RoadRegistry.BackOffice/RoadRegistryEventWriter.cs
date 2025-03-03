namespace RoadRegistry.BackOffice;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.Generators.Guid;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Framework;
using Messages;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;

public abstract class RoadRegistryEventWriter
{
    protected static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    private readonly EventEnricher _enricher;
    private readonly IStreamStore _store;
    private readonly EventMapping _eventMapping;

    protected RoadRegistryEventWriter(IStreamStore store, EventEnricher enricher)
        : this(store, enricher, EventMapping)
    {
    }

    protected RoadRegistryEventWriter(IStreamStore store, EventEnricher enricher, EventMapping eventMapping)
    {
        _store = store.ThrowIfNull();
        _enricher = enricher.ThrowIfNull();
        _eventMapping = eventMapping.ThrowIfNull();
    }

    protected Task AppendToStoreStream(StreamName streamName, IRoadRegistryMessage message, int expectedVersion, object[] events, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(streamName, message.MessageId, expectedVersion, events, message.ProvenanceData, cancellationToken);
    }

    protected async Task AppendToStoreStream(StreamName streamName, Guid messageId, int expectedVersion, object[] events, ProvenanceData provenanceData, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(events);

        Array.ForEach(events, @event => _enricher(@event));

        var version = expectedVersion;

        var messages = Array.ConvertAll(
            events,
            @event =>
                new NewStreamMessage(
                    Deterministic.Create(Deterministic.Namespaces.Events,
                        $"{messageId:N}-{version++}"),
                    _eventMapping.GetEventName(@event.GetType()),
                    JsonConvert.SerializeObject(@event, SerializerSettings),
                    SerializeJsonMetadata(provenanceData)
                ));

        await _store.AppendToStream(streamName, expectedVersion, messages, cancellationToken);
    }

    private static string SerializeJsonMetadata(ProvenanceData provenanceData)
    {
        var jsonMetadata = JsonConvert.SerializeObject(new MessageMetadata
        {
            ProvenanceData = provenanceData
        }, SerializerSettings);
        return jsonMetadata;
    }
}
