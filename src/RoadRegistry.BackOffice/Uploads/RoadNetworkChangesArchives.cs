namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;

public class RoadNetworkChangesArchives : IRoadNetworkChangesArchives
{
    private readonly EventSourcedEntityMap _map;
    private readonly EventMapping _mapping;
    private readonly JsonSerializerSettings _settings;
    private readonly IStreamStore _store;

    public RoadNetworkChangesArchives(EventSourcedEntityMap map, IStreamStore store, JsonSerializerSettings settings, EventMapping mapping)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
    }

    public void Add(RoadNetworkChangesArchive archive)
    {
        ArgumentNullException.ThrowIfNull(archive);
        
        _map.Attach(new EventSourcedEntityMapEntry(archive, new StreamName(archive.Id), ExpectedVersion.NoStream));
    }

    public static StreamName GetStreamName(ArchiveId id)
    {
        return new StreamName(id);
    }

    public async Task<RoadNetworkChangesArchive> Get(ArchiveId id, CancellationToken ct = default)
    {
        var stream = GetStreamName(id);
        if (_map.TryGet(stream, out var entry)) return (RoadNetworkChangesArchive)entry.Entity;
        var page = await _store.ReadStreamForwards(stream, StreamVersion.Start, 1024, ct);
        if (page.Status == PageReadStatus.StreamNotFound)
        {
            var network = RoadNetworkChangesArchive.Factory();
            _map.Attach(new EventSourcedEntityMapEntry(network, stream, ExpectedVersion.NoStream));
            return network;
        }

        IEventSourcedEntity entity = RoadNetworkChangesArchive.Factory();
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
            if (page.Status == PageReadStatus.StreamNotFound)
            {
                var network = RoadNetworkChangesArchive.Factory();
                _map.Attach(new EventSourcedEntityMapEntry(network, stream, ExpectedVersion.NoStream));
                return network;
            }

            foreach (var message in page.Messages)
                messages.Add(
                    JsonConvert.DeserializeObject(
                        await message.GetJsonData(ct),
                        _mapping.GetEventType(message.Type),
                        _settings));
            entity.RestoreFromEvents(messages.ToArray());
        }

        _map.Attach(new EventSourcedEntityMapEntry(entity, stream, page.LastStreamVersion));
        return (RoadNetworkChangesArchive)entity;
    }
}
