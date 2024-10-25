namespace RoadRegistry.BackOffice.Extracts;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;

public class RoadNetworkExtracts : IRoadNetworkExtracts
{
    public static readonly StreamName Prefix = new("extract-");
    private readonly EventSourcedEntityMap _map;
    private readonly EventMapping _mapping;
    private readonly EventEnricher _eventEnricher;
    private readonly JsonSerializerSettings _settings;
    private readonly IStreamStore _store;

    public RoadNetworkExtracts(EventSourcedEntityMap map, IStreamStore store, JsonSerializerSettings settings, EventMapping mapping, EventEnricher eventEnricher)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
        _eventEnricher = eventEnricher.ThrowIfNull();
    }

    public void Add(RoadNetworkExtract extract)
    {
        if (extract == null)
            throw new ArgumentNullException(nameof(extract));

        _map.Attach(new EventSourcedEntityMapEntry(extract, ToStreamName(extract.Id), ExpectedVersion.NoStream));
    }

    public async Task<RoadNetworkExtract> Get(ExtractRequestId id, CancellationToken ct = default)
    {
        var stream = ToStreamName(id);
        if (_map.TryGet(stream, out var entry)) return (RoadNetworkExtract)entry.Entity;
        var page = await _store.ReadStreamForwards(stream, StreamVersion.Start, 1024, ct);
        if (page.Status == PageReadStatus.StreamNotFound) return null;
        IEventSourcedEntity entity = RoadNetworkExtract.Factory(_eventEnricher);
        var messages = new List<object>(page.Messages.Length);
        foreach (var message in page.Messages)
            messages.Add(
                JsonConvert.DeserializeObject(
                    await message.GetJsonData(ct),
                    _mapping.GetEventType(message.Type),
                    _settings));
        entity.RestoreFromEvents(messages.ToArray());
        while (!page.IsEnd)
        {
            messages.Clear();
            page = await page.ReadNext(ct);
            if (page.Status == PageReadStatus.StreamNotFound) return null;
            foreach (var message in page.Messages)
                messages.Add(
                    JsonConvert.DeserializeObject(
                        await message.GetJsonData(ct),
                        _mapping.GetEventType(message.Type),
                        _settings));
            entity.RestoreFromEvents(messages.ToArray());
        }

        _map.Attach(new EventSourcedEntityMapEntry(entity, stream, page.LastStreamVersion));
        return (RoadNetworkExtract)entity;
    }

    public static StreamName ToStreamName(ExtractRequestId id)
    {
        return Prefix.WithSuffix(id.ToString());
    }
}
