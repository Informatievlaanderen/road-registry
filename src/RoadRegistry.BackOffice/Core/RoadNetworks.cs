namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Framework;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public class RoadNetworks : IRoadNetworks
    {
        private const int StreamPageSize = 5_000;

        public static readonly StreamName Stream = new StreamName("roadnetwork");

        private readonly EventSourcedEntityMap _map;
        private readonly IStreamStore _store;
        private readonly JsonSerializerSettings _settings;
        private readonly EventMapping _mapping;
        private readonly IRoadNetworkSnapshotReader _snapshotReader;

        public RoadNetworks(EventSourcedEntityMap map, IStreamStore store, IRoadNetworkSnapshotReader snapshotReader,
            JsonSerializerSettings settings, EventMapping mapping)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
            _snapshotReader = snapshotReader ?? throw new ArgumentNullException(nameof(snapshotReader));
        }

        public async Task<RoadNetwork> Get(CancellationToken ct = default)
        {
            if (_map.TryGet(Stream, out var entry))
            {
                return (RoadNetwork)entry.Entity;
            }

            var roadNetwork = RoadNetwork.Factory();
            var (snapshot, version) = await _snapshotReader.ReadSnapshot(ct);
            if (version != ExpectedVersion.NoStream)
            {
                roadNetwork.RestoreFromSnapshot(snapshot);
                version += 1;
            }
            else
            {
                version = StreamVersion.Start;
            }
            var page = await _store.ReadStreamForwards(Stream, version, StreamPageSize, ct);
            if (page.Status == PageReadStatus.StreamNotFound)
            {
                var initial = RoadNetwork.Factory();
                _map.Attach(new EventSourcedEntityMapEntry(initial, Stream, ExpectedVersion.NoStream));
                return initial;
            }
            IEventSourcedEntity entity = roadNetwork;
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
                    var initial = RoadNetwork.Factory();
                    _map.Attach(new EventSourcedEntityMapEntry(initial, Stream, ExpectedVersion.NoStream));
                    return initial;
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
            return roadNetwork;
        }

        public async Task<(RoadNetwork, int)> GetWithVersion(CancellationToken ct = default)
        {
            if (_map.TryGet(Stream, out var entry))
            {
                return ((RoadNetwork)entry.Entity, entry.ExpectedVersion);
            }
            var roadNetwork = RoadNetwork.Factory();
            var (snapshot, version) = await _snapshotReader.ReadSnapshot(ct);
            if (version != ExpectedVersion.NoStream)
            {
                roadNetwork.RestoreFromSnapshot(snapshot);
                version += 1;
            }
            else
            {
                version = StreamVersion.Start;
            }

            var page = await _store.ReadStreamForwards(Stream, version, StreamPageSize, ct);
            if (page.Status == PageReadStatus.StreamNotFound)
            {
                var network = RoadNetwork.Factory();
                _map.Attach(new EventSourcedEntityMapEntry(network, Stream, ExpectedVersion.NoStream));
                return (network, ExpectedVersion.NoStream);
            }
            IEventSourcedEntity entity = roadNetwork;
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
                    return (network, ExpectedVersion.NoStream);
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
            return (roadNetwork, page.LastStreamVersion);
        }
    }
}
