namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.AggregateSource;
    using Aiv.Vbr.EventHandling;
    using Framework;
    using Model;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public class RoadNetworks : IRoadNetworks
    {
        public static readonly StreamName Stream = new StreamName("roadnetwork");

        private readonly EventSourcedEntityMap _map;
        private readonly IStreamStore _store;
        private readonly JsonSerializerSettings _settings;
        private readonly EventMapping _mapping;

        public RoadNetworks(EventSourcedEntityMap map, IStreamStore store, JsonSerializerSettings settings, EventMapping mapping)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
        }

        public async Task<RoadNetwork> Get(CancellationToken ct = default)
        {
            if (_map.TryGet(Stream, out var entry))
            {
                return (RoadNetwork)entry.Entity;
            }
            var page = await _store.ReadStreamForwards(Stream, StreamVersion.Start, 1024, ct);
            if (page.Status == PageReadStatus.StreamNotFound)
            {
                var network = RoadNetwork.Factory();
                _map.Attach(new EventSourcedEntityMapEntry(network, Stream, ExpectedVersion.NoStream));
                return network;
            }
            IEventSourcedEntity entity = RoadNetwork.Factory();
            var messages = new List<object>(page.Messages.Length);
            foreach (var message in page.Messages)
            {
                messages.Add(
                    JsonConvert.DeserializeObject(
                        await message.GetJsonData(ct),
                        _mapping.GetEventType(message.Type),
                        _settings));
            }
            entity.RestoreFromEvents(messages.ToArray());
            while (!page.IsEnd)
            {
                messages.Clear();
                page = await page.ReadNext(ct);
                if (page.Status == PageReadStatus.StreamNotFound)
                {
                    var network = RoadNetwork.Factory();
                    _map.Attach(new EventSourcedEntityMapEntry(network, Stream, ExpectedVersion.NoStream));
                    return network;
                }
                foreach (var message in page.Messages)
                {
                    messages.Add(
                        JsonConvert.DeserializeObject(
                            await message.GetJsonData(ct),
                            _mapping.GetEventType(message.Type),
                            _settings));
                }
                entity.RestoreFromEvents(messages.ToArray());
            }
            _map.Attach(new EventSourcedEntityMapEntry(entity, Stream, page.LastStreamVersion));
            return (RoadNetwork)entity;
        }
    }
}
