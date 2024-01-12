namespace RoadRegistry.BackOffice;

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.Generators.Guid;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Framework;
using Newtonsoft.Json;
using SqlStreamStore.Streams;
using SqlStreamStore;
using Claim = Framework.Claim;

public abstract class RoadRegistryEventWriter
{
    protected static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    private readonly EventEnricher _enricher;
    private readonly IStreamStore _store;
    private readonly EventMapping _eventMapping;

    protected RoadRegistryEventWriter(IStreamStore store, EventEnricher enricher, EventMapping eventMapping)
    {
        _store = store.ThrowIfNull();
        _enricher = enricher.ThrowIfNull();
        _eventMapping = eventMapping.ThrowIfNull();
    }

    protected Task AppendToStoreStream(StreamName streamName, IRoadRegistryMessage message, int expectedVersion, object[] events, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(streamName, message.MessageId, expectedVersion, events, message.Principal, message.ProvenanceData, cancellationToken);
    }

    protected async Task AppendToStoreStream(StreamName streamName, Guid messageId, int expectedVersion, object[] events, ClaimsPrincipal principal, ProvenanceData provenanceData, CancellationToken cancellationToken)
    {
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
                    principal != null ? SerializeJsonMetadata(principal, provenanceData) : null
                ));

        await _store.AppendToStream(streamName, expectedVersion, messages, cancellationToken);
    }

    private static string SerializeJsonMetadata(ClaimsPrincipal principal, ProvenanceData provenanceData)
    {
        var jsonMetadata = JsonConvert.SerializeObject(new MessageMetadata
        {
            Principal = principal
                .Claims
                .Select(claim => new Claim { Type = claim.Type, Value = claim.Value })
                .ToArray(),
            ProvenanceData = provenanceData
        }, SerializerSettings);
        return jsonMetadata;
    }
}
