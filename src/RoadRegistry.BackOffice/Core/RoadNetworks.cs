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

    public async Task<RoadNetwork> Get(bool restoreSnapshot, int maximumStreamVersion, CancellationToken cancellationToken)
    {
        if (_map.TryGet(Stream, out var entry)) return (RoadNetwork)entry.Entity;

        var (view, version) = await BuildInitialRoadNetworkView(restoreSnapshot, cancellationToken);

        var messagesMaxCount = version == StreamVersion.Start ? maximumStreamVersion : maximumStreamVersion - version;

        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Read stream forward started with {Stream}, version {SnapshotVersion} and page size {StreamPageSize}", Stream, version, StreamPageSize);
        var page = await _store.ReadStreamForwards(Stream, version, messagesMaxCount, cancellationToken);
        _logger.LogInformation("Read stream forward finished with {Stream}, version {SnapshotVersion} and page size {StreamPageSize} in {StopwatchElapsedMilliseconds}ms", Stream, version, StreamPageSize, sw.ElapsedMilliseconds);

        if (page.Status == PageReadStatus.StreamNotFound)
        {
            var (emptyRoadNetwork, _) = EmptyRoadNetwork();
            return emptyRoadNetwork;
        }

        view = await ProcessPages(view, page, cancellationToken);
        
        var roadNetwork = RoadNetwork.Factory(view.ToImmutable());
        _map.Attach(new EventSourcedEntityMapEntry(roadNetwork, Stream, page.LastStreamVersion));
        return roadNetwork;
    }

    public async Task<RoadNetwork> Get(CancellationToken cancellationToken)
    {
        var (roadNetwork, _) = await GetWithVersion(cancellationToken);
        return roadNetwork;
    }
    public async Task<RoadNetwork> Get(bool restoreSnapshot, CancellationToken cancellationToken)
    {
        var (roadNetwork, _) = await GetWithVersion(restoreSnapshot, cancellationToken);
        return roadNetwork;
    }

    public Task<(RoadNetwork, int)> GetWithVersion(CancellationToken cancellationToken)
    {
        return GetWithVersion(true, cancellationToken);
    }

    public async Task<(RoadNetwork, int)> GetWithVersion(bool restoreSnapshot, CancellationToken cancellationToken)
    {
        if (_map.TryGet(Stream, out var entry)) return ((RoadNetwork)entry.Entity, entry.ExpectedVersion);

        var (view, version) = await BuildInitialRoadNetworkView(restoreSnapshot, cancellationToken);

        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Read stream forward started with {Stream}, version {SnapshotVersion} and page size {StreamPageSize}", Stream, version, StreamPageSize);
        var page = await _store.ReadStreamForwards(Stream, version, StreamPageSize, cancellationToken);
        _logger.LogInformation("Read stream forward finished with {Stream}, version {SnapshotVersion} and page size {StreamPageSize} in {StopwatchElapsedMilliseconds}ms", Stream, version, StreamPageSize, sw.ElapsedMilliseconds);

        if (page.Status == PageReadStatus.StreamNotFound)
        {
            return EmptyRoadNetwork();
        }

        view = await ProcessPages(view, page, cancellationToken);

        var roadNetwork = RoadNetwork.Factory(view.ToImmutable());
        _map.Attach(new EventSourcedEntityMapEntry(roadNetwork, Stream, page.LastStreamVersion));
        
        return (roadNetwork, page.LastStreamVersion);
    }

    private (RoadNetwork, int) EmptyRoadNetwork()
    {
        var initial = RoadNetwork.Factory(ImmutableRoadNetworkView.Empty);
        _map.Attach(new EventSourcedEntityMapEntry(initial, Stream, ExpectedVersion.NoStream));
        return (initial, ExpectedVersion.NoStream);
    }

    private async Task<IRoadNetworkView> ProcessPages(IRoadNetworkView view, ReadStreamPage page, CancellationToken cancellationToken)
    {
        view = await ProcessPage(view, page, cancellationToken);
        
        var sw = Stopwatch.StartNew();
        while (!page.IsEnd)
        {
            sw.Restart();
            _logger.LogInformation("Next page read started with {Stream}, from version {FromStreamVersion} until {LastStreamVersion} and page size {StreamPageSize}", Stream, page.FromStreamVersion, page.LastStreamVersion, StreamPageSize);
            page = await page.ReadNext(cancellationToken);
            _logger.LogInformation("Next page read finished with {Stream}, from version {FromStreamVersion} until {LastStreamVersion} and page size {StreamPageSize} in {StopwatchElapsedMilliseconds}ms", Stream, page.FromStreamVersion, page.LastStreamVersion, StreamPageSize, sw.ElapsedMilliseconds);

            if (page.Status == PageReadStatus.StreamNotFound)
            {
                throw new InvalidOperationException($"Page status {page.Status} encountered while processing consecutive pages");
            }

            view = await ProcessPage(view, page, cancellationToken);
        }

        return view;
    }

    private async Task<IRoadNetworkView> ProcessPage(IRoadNetworkView view, ReadStreamPage page, CancellationToken cancellationToken)
    {
        var messages = new List<object>(page.Messages.Length);

        foreach (var message in page.Messages)
            messages.Add(
                JsonConvert.DeserializeObject(
                    await message.GetJsonData(cancellationToken),
                    _mapping.GetEventType(message.Type),
                    _settings));

        messages.TryGetNonEnumeratedCount(out var messageCountInternal);

        var sw = Stopwatch.StartNew();
        _logger.LogInformation("View restore from events started with {MessageCount} messages", messageCountInternal);
        try
        {
            view = view.RestoreFromEvents(messages.ToArray());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed trying to process the messages between StreamVersion {page.FromStreamVersion} and {page.LastStreamVersion}: {ex.Message}", ex);
        }
        _logger.LogInformation("View restore from events finished with {MessageCount} messages in {StopwatchElapsedMilliseconds}ms", messageCountInternal, sw.ElapsedMilliseconds);

        return view;
    }

    private async Task<(IRoadNetworkView, int)> BuildInitialRoadNetworkView(bool restoreSnapshot, CancellationToken cancellationToken)
    {
        var view = ImmutableRoadNetworkView.Empty.ToBuilder();

        var sw = Stopwatch.StartNew();

        int version;

        if (restoreSnapshot)
        {
            _logger.LogInformation("Read started for RoadNetwork snapshot");
            var (snapshot, snapshotVersion) = await _snapshotReader.ReadSnapshotAsync(cancellationToken);
            version = snapshotVersion;
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
        }
        else
        {
            version = StreamVersion.Start;
        }

        return (view, version);
    }
}
