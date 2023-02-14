namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<RoadNetworks> _logger;

    public RoadNetworks(
        EventSourcedEntityMap map,
        IStreamStore store,
        IRoadNetworkSnapshotReader snapshotReader,
        JsonSerializerSettings settings,
        EventMapping mapping,
        ILogger<RoadNetworks> logger)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
        _snapshotReader = snapshotReader ?? throw new ArgumentNullException(nameof(snapshotReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RoadNetwork> Get(int maxStreamVersion, CancellationToken ct = default)
    {
        if (_map.TryGet(Stream, out var entry)) return (RoadNetwork)entry.Entity;

        var view = ImmutableRoadNetworkView.Empty.ToBuilder();

        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Read started for RoadNetwork snapshot");
        var (snapshot, version) = await _snapshotReader.ReadSnapshot(ct);
        _logger.LogInformation("Read finished for RoadNetwork snapshot version {SnapshotVersion} in {StopwatchElapsedMilliseconds}ms", version, sw.ElapsedMilliseconds);

        if (version != ExpectedVersion.NoStream)
        {
            sw.Restart();
            _logger.LogInformation("View restore started from RoadNetwork snapshot version {SnapshotVersion}", version);
            view = view.RestoreFromSnapshot(snapshot);
            _logger.LogInformation("View restore finished for RoadNetwork snapshot version {SnapshotVersion} in {StopwatchElapsedMilliseconds}ms", version, sw.ElapsedMilliseconds);
            version += 1;
        }
        else
        {
            version = StreamVersion.Start;
        }

        sw.Restart();
        _logger.LogInformation("Read stream forward started with {Stream}, version {SnapshotVersion} and page size {StreamPageSize}", Stream, version, StreamPageSize);
        var page = await _store.ReadStreamForwards(Stream, version, maxStreamVersion - version, ct);
        _logger.LogInformation("Read stream forward finished with {Stream}, version {SnapshotVersion} and page size {StreamPageSize} in {StopwatchElapsedMilliseconds}ms", Stream, version, StreamPageSize, sw.ElapsedMilliseconds);

        if (page.Status == PageReadStatus.StreamNotFound)
        {
            var initial = RoadNetwork.Factory(ImmutableRoadNetworkView.Empty);
            _map.Attach(new EventSourcedEntityMapEntry(initial, Stream, ExpectedVersion.NoStream));
            return initial;
        }

        var messages = new List<object>(page.Messages.Length);

        sw.Restart();
        _logger.LogInformation("Read stream forward started with {Stream}, version {SnapshotVersion} and page size {StreamPageSize}", Stream, version, StreamPageSize);
        foreach (var message in page.Messages)
            messages.Add(
                JsonConvert.DeserializeObject(
                    await message.GetJsonData(ct),
                    _mapping.GetEventType(message.Type),
                    _settings));
        _logger.LogInformation("Read stream forward finished with {Stream}, version {SnapshotVersion} and page size {StreamPageSize} in {StopwatchElapsedMilliseconds}ms", Stream, version, StreamPageSize, sw.ElapsedMilliseconds);

        sw.Restart();
        messages.TryGetNonEnumeratedCount(out var messageCount);
        _logger.LogInformation("View restore from events started with {MessageCount} messages", messageCount);
        view = view.RestoreFromEvents(messages.ToArray());
        _logger.LogInformation("View restore from events finished with {MessageCount} messages in {StopwatchElapsedMilliseconds}ms", messageCount, sw.ElapsedMilliseconds);

        var roadNetwork = RoadNetwork.Factory(view.ToImmutable());
        _map.Attach(new EventSourcedEntityMapEntry(roadNetwork, Stream, page.LastStreamVersion));
        return roadNetwork;
    }

    public async Task<RoadNetwork> Get(CancellationToken ct = default)
    {
        if (_map.TryGet(Stream, out var entry)) return (RoadNetwork)entry.Entity;

        var view = ImmutableRoadNetworkView.Empty.ToBuilder();

        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Read started for RoadNetwork snapshot");
        var (snapshot, version) = await _snapshotReader.ReadSnapshot(ct);
        _logger.LogInformation("Read finished for RoadNetwork snapshot version {SnapshotVersion} in {StopwatchElapsedMilliseconds}ms", version, sw.ElapsedMilliseconds);

        if (version != ExpectedVersion.NoStream)
        {
            sw.Restart();
            _logger.LogInformation("View restore started from RoadNetwork snapshot version {SnapshotVersion}", version);
            view = view.RestoreFromSnapshot(snapshot);
            _logger.LogInformation("View restore finished for RoadNetwork snapshot version {SnapshotVersion} in {StopwatchElapsedMilliseconds}ms", version, sw.ElapsedMilliseconds);
            version += 1;
        }
        else
        {
            version = StreamVersion.Start;
        }

        sw.Restart();
        _logger.LogInformation("Read stream forward started with {Stream}, version {SnapshotVersion} and page size {StreamPageSize}", Stream, version, StreamPageSize);
        var page = await _store.ReadStreamForwards(Stream, version, StreamPageSize, ct);
        _logger.LogInformation("Read stream forward finished with {Stream}, version {SnapshotVersion} and page size {StreamPageSize} in {StopwatchElapsedMilliseconds}ms", Stream, version, StreamPageSize, sw.ElapsedMilliseconds);

        if (page.Status == PageReadStatus.StreamNotFound)
        {
            var initial = RoadNetwork.Factory(ImmutableRoadNetworkView.Empty);
            _map.Attach(new EventSourcedEntityMapEntry(initial, Stream, ExpectedVersion.NoStream));
            return initial;
        }

        var messages = new List<object>(page.Messages.Length);

        sw.Restart();
        _logger.LogInformation("Read stream forward started with {Stream}, version {SnapshotVersion} and page size {StreamPageSize}", Stream, version, StreamPageSize);
        foreach (var message in page.Messages)
            messages.Add(
                JsonConvert.DeserializeObject(
                    await message.GetJsonData(ct),
                    _mapping.GetEventType(message.Type),
                    _settings));
        _logger.LogInformation("Read stream forward finished with {Stream}, version {SnapshotVersion} and page size {StreamPageSize} in {StopwatchElapsedMilliseconds}ms", Stream, version, StreamPageSize, sw.ElapsedMilliseconds);

        sw.Restart();
        messages.TryGetNonEnumeratedCount(out var messageCount);
        _logger.LogInformation("View restore from events started with {MessageCount} messages", messageCount);
        view = view.RestoreFromEvents(messages.ToArray());
        _logger.LogInformation("View restore from events finished with {MessageCount} messages in {StopwatchElapsedMilliseconds}ms", messageCount, sw.ElapsedMilliseconds);

        while (!page.IsEnd)
        {
            messages.Clear();

            sw.Restart();
            _logger.LogInformation("Next page read started with {Stream}, version {SnapshotVersion} and page size {StreamPageSize}", Stream, version, StreamPageSize);
            page = await page.ReadNext(ct);
            _logger.LogInformation("Next page read finished with {Stream}, version {SnapshotVersion} and page size {StreamPageSize} in {StopwatchElapsedMilliseconds}ms", Stream, version, StreamPageSize, sw.ElapsedMilliseconds);

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

            messages.TryGetNonEnumeratedCount(out var messageCountInternal);

            sw.Restart();
            _logger.LogInformation("View restore from events started with {MessageCount} messages", messageCountInternal);
            view = view.RestoreFromEvents(messages.ToArray());
            _logger.LogInformation("View restore from events finished with {MessageCount} messages in {StopwatchElapsedMilliseconds}ms", messageCountInternal, sw.ElapsedMilliseconds);
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
