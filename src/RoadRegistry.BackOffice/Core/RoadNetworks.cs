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

public class RoadNetworks : IRoadNetworks
{
    private const int StreamPageSize = 100;

    public static readonly StreamName Stream = new("roadnetwork");

    private readonly EventSourcedEntityMap _map;
    private readonly EventMapping _mapping;
    private readonly JsonSerializerSettings _settings;
    private readonly IRoadNetworkSnapshotReader _snapshotReader;
    private readonly IStreamStore _store;

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
        if (_map.TryGet(Stream, out var entry)) return (RoadNetwork)entry.Entity;

        var view = ImmutableRoadNetworkView.Empty.ToBuilder();
        var (snapshot, version) = await _snapshotReader.ReadSnapshot(ct);
        if (version != ExpectedVersion.NoStream)
        {
            view = view.RestoreFromSnapshot(snapshot);
            version += 1;
        }
        else
        {
            version = StreamVersion.Start;
        }

        var page = await _store.ReadStreamForwards(Stream, version, StreamPageSize, ct);
        if (page.Status == PageReadStatus.StreamNotFound)
        {
            var initial = RoadNetwork.Factory(ImmutableRoadNetworkView.Empty);
            _map.Attach(new EventSourcedEntityMapEntry(initial, Stream, ExpectedVersion.NoStream));
            return initial;
        }

        var messages = new List<object>(page.Messages.Length);
        foreach (var message in page.Messages)
            messages.Add(
                JsonConvert.DeserializeObject(
                    await message.GetJsonData(ct),
                    _mapping.GetEventType(message.Type),
                    _settings));
        view = view.RestoreFromEvents(messages.ToArray());
        while (!page.IsEnd)
        {
            messages.Clear();
            page = await page.ReadNext(ct);
            if (page.Status == PageReadStatus.StreamNotFound)
            {
                var initial = RoadNetwork.Factory(ImmutableRoadNetworkView.Empty);
                _map.Attach(new EventSourcedEntityMapEntry(initial, Stream, ExpectedVersion.NoStream));
                return initial;
            }

            foreach (var message in page.Messages)
                messages.Add(
                    JsonConvert.DeserializeObject(
                        await message.GetJsonData(ct),
                        _mapping.GetEventType(message.Type),
                        _settings));
            view = view.RestoreFromEvents(messages.ToArray());
        }

        var roadNetwork = RoadNetwork.Factory(view.ToImmutable());
        _map.Attach(new EventSourcedEntityMapEntry(roadNetwork, Stream, page.LastStreamVersion));
        return roadNetwork;
    }

    public async Task<(RoadNetwork, int)> GetWithVersion(CancellationToken ct = default)
    {
        if (_map.TryGet(Stream, out var entry)) return ((RoadNetwork)entry.Entity, entry.ExpectedVersion);
        var view = ImmutableRoadNetworkView.Empty.ToBuilder();
        var (snapshot, version) = await _snapshotReader.ReadSnapshot(ct);
        if (version != ExpectedVersion.NoStream)
        {
            view = view.RestoreFromSnapshot(snapshot);
            version += 1;
        }
        else
        {
            version = StreamVersion.Start;
        }

        var page = await _store.ReadStreamForwards(Stream, version, StreamPageSize, ct);
        if (page.Status == PageReadStatus.StreamNotFound)
        {
            var network = RoadNetwork.Factory(ImmutableRoadNetworkView.Empty);
            _map.Attach(new EventSourcedEntityMapEntry(network, Stream, ExpectedVersion.NoStream));
            return (network, ExpectedVersion.NoStream);
        }

        var messages = new List<object>(page.Messages.Length);
        foreach (var message in page.Messages)
            messages.Add(
                JsonConvert.DeserializeObject(
                    await message.GetJsonData(ct),
                    _mapping.GetEventType(message.Type),
                    _settings));
        view = view.RestoreFromEvents(messages.ToArray());
        while (!page.IsEnd)
        {
            messages.Clear();
            page = await page.ReadNext(ct);
            if (page.Status == PageReadStatus.StreamNotFound)
            {
                var network = RoadNetwork.Factory(ImmutableRoadNetworkView.Empty);
                _map.Attach(new EventSourcedEntityMapEntry(network, Stream, ExpectedVersion.NoStream));
                return (network, ExpectedVersion.NoStream);
            }

            foreach (var message in page.Messages)
                messages.Add(
                    JsonConvert.DeserializeObject(
                        await message.GetJsonData(ct),
                        _mapping.GetEventType(message.Type),
                        _settings));
            view = view.RestoreFromEvents(messages.ToArray());
        }

        var roadNetwork = RoadNetwork.Factory(view.ToImmutable());
        _map.Attach(new EventSourcedEntityMapEntry(roadNetwork, Stream, page.LastStreamVersion));
        return (roadNetwork, page.LastStreamVersion);
    }
}
