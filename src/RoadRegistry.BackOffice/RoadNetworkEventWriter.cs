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
using Messages;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;
using Claim = Framework.Claim;

public interface IRoadNetworkEventWriter
{
    Task WriteAsync(StreamName streamName, Guid messageId, int version, object[] events, CancellationToken cancellationToken);
    Task WriteAsync(StreamName streamName, IRoadRegistryMessage message, int expectedVersion, object[] events, CancellationToken cancellationToken);
}

public class RoadNetworkEventWriter : IRoadNetworkEventWriter
{
    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    private static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    private readonly EventEnricher _enricher;
    private readonly IStreamStore _store;

    public RoadNetworkEventWriter(IStreamStore store, EventEnricher enricher)
    {
        _store = store;
        _enricher = enricher;
    }

    public Task WriteAsync(StreamName streamName, Guid messageId, int expectedVersion, object[] events, CancellationToken cancellationToken)
    {
        return Write(streamName, messageId, expectedVersion, events, null, null, cancellationToken);
    }

    public Task WriteAsync(StreamName streamName, IRoadRegistryMessage message, int expectedVersion, object[] events, CancellationToken cancellationToken)
    {
        return Write(streamName, message.MessageId, expectedVersion, events, message.Principal, message.ProvenanceData, cancellationToken);
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

    private async Task Write(StreamName streamName, Guid messageId, int expectedVersion, object[] events, ClaimsPrincipal principal, ProvenanceData provenanceData, CancellationToken cancellationToken)
    {
        Array.ForEach(events, @event => _enricher(@event));

        var version = expectedVersion;

        var messages = Array.ConvertAll(
            events,
            @event =>
                new NewStreamMessage(
                    Deterministic.Create(Deterministic.Namespaces.Events,
                        $"{messageId:N}-{version++}"),
                    EventMapping.GetEventName(@event.GetType()),
                    JsonConvert.SerializeObject(@event, SerializerSettings),
                    principal != null ? SerializeJsonMetadata(principal, provenanceData) : null
                ));

        await _store.AppendToStream(streamName, expectedVersion, messages, cancellationToken);
    }
}
