namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;

public abstract class EventSourcedEntityRepository<TEntity, TIdentifier>
    where TEntity: class, IEventSourcedEntity
{
    private readonly Func<TEntity> _entityFactory;
    private readonly EventSourcedEntityMap _map;
    private readonly EventMapping _mapping;
    private readonly JsonSerializerSettings _settings;
    private readonly IStreamStore _store;
    private readonly Func<TIdentifier, StreamName> _getStreamName;

    protected EventSourcedEntityRepository(
        EventSourcedEntityMap map,
        IStreamStore store,
        JsonSerializerSettings settings,
        EventMapping mapping,
        Func<TIdentifier, StreamName> getStreamName,
        Func<TEntity> entityFactory
    )
    {
        _map = map.ThrowIfNull();
        _store = store.ThrowIfNull();
        _settings = settings.ThrowIfNull();
        _mapping = mapping.ThrowIfNull();
        _getStreamName = getStreamName.ThrowIfNull();
        _entityFactory = entityFactory.ThrowIfNull();
    }

    public async Task<TEntity> FindAsync(TIdentifier id, CancellationToken ct = default)
    {
        var stream = _getStreamName(id);
        if (_map.TryGet(stream, out var entry))
        {
            var cachedEntity = (TEntity)entry.Entity;
            return ConvertEntity(cachedEntity);
        }

        var page = await _store.ReadStreamForwards(stream, StreamVersion.Start, 100, ct);
        if (page.Status == PageReadStatus.StreamNotFound || !page.Messages.Any())
        {
            return null;
        }

        var entity = _entityFactory();
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
                return null;
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

        return ConvertEntity(entity);
    }

    protected virtual TEntity ConvertEntity(TEntity entity)
    {
        return entity;
    }
}
