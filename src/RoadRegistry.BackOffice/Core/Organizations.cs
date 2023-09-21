namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;

public class Organizations : IOrganizations
{
    public static readonly Func<OrganizationId, StreamName> ToStreamName = OrganizationId.ToStreamName;

    private readonly EventSourcedEntityMap _map;
    private readonly EventMapping _mapping;
    private readonly JsonSerializerSettings _settings;
    private readonly IStreamStore _store;

    public Organizations(EventSourcedEntityMap map, IStreamStore store, JsonSerializerSettings settings, EventMapping mapping)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
    }

    public async Task<Organization> FindAsync(OrganizationId id, CancellationToken ct = default)
    {
        var stream = ToStreamName(id);
        if (_map.TryGet(stream, out var entry))
        {
            var cachedOrganization = (Organization)entry.Entity;
            return cachedOrganization.IsRemoved ? null : cachedOrganization;
        }

        var organization = Organization.Factory();
        var page = await _store.ReadStreamForwards(stream, StreamVersion.Start, 100, ct);
        if (page.Status == PageReadStatus.StreamNotFound) return null;
        IEventSourcedEntity entity = organization;
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

        return organization.IsRemoved ? null : organization;
    }
}
