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

public delegate bool ProcessMessageHandler(int messageStreamVersion, int pageLastStreamVersion);

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

    private sealed class ProcessSnapshotContext
    {
        private int? _version;
        public int? Version
        {
            get => _version;
            set
            {
                if (_version == null || value > _version)
                {
                    _version = value;
                }
            }
        }

        public bool ProcessingCancelled;

        public ProcessSnapshotContext(int? version)
        {
            Version = version;
        }
    }

    public async Task<RoadNetwork> Get(CancellationToken cancellationToken)
    {
        var (roadNetwork, _) = await GetWithVersion(cancellationToken);
        return roadNetwork;
    }
    public async Task<RoadNetwork> Get(bool restoreSnapshot, ProcessMessageHandler processMessage, CancellationToken cancellationToken)
    {
        var (roadNetwork, _) = await GetWithVersion(restoreSnapshot, processMessage, cancellationToken);
        return roadNetwork;
    }

    public Task<(RoadNetwork, int)> GetWithVersion(CancellationToken cancellationToken)
    {
        return GetWithVersion(true, null, cancellationToken);
    }

    public async Task<(RoadNetwork, int)> GetWithVersion(bool restoreSnapshot, ProcessMessageHandler processMessage, CancellationToken cancellationToken)
    {
        if (_map.TryGet(Stream, out var entry)) return ((RoadNetwork)entry.Entity, entry.ExpectedVersion);

        var (view, version) = await BuildInitialRoadNetworkView(restoreSnapshot, cancellationToken);

        var sw = Stopwatch.StartNew();

        var readStreamFromVersion = version == StreamVersion.Start ? version : version + 1;
        _logger.LogInformation("Read stream forward started with {Stream}, version {SnapshotVersion} and page size {StreamPageSize}", Stream, readStreamFromVersion, StreamPageSize);
        var page = await _store.ReadStreamForwards(Stream, readStreamFromVersion, StreamPageSize, cancellationToken);
        _logger.LogInformation("Read stream forward finished with {Stream}, version {SnapshotVersion} and page size {StreamPageSize} in {StopwatchElapsedMilliseconds}ms", Stream, readStreamFromVersion, StreamPageSize, sw.ElapsedMilliseconds);

        if (page.Status == PageReadStatus.StreamNotFound)
        {
            return EmptyRoadNetwork();
        }

        var snapshotContext = new ProcessSnapshotContext(version);
        view = await ProcessPages(view, snapshotContext, page, processMessage, cancellationToken);
        
        var roadNetwork = RoadNetwork.Factory(view.ToImmutable());
        _map.Attach(new EventSourcedEntityMapEntry(roadNetwork, Stream, ExpectedVersion.Any));
        
        return (roadNetwork, snapshotContext.Version.Value);
    }

    private (RoadNetwork, int) EmptyRoadNetwork()
    {
        var initial = RoadNetwork.Factory(ImmutableRoadNetworkView.Empty);
        _map.Attach(new EventSourcedEntityMapEntry(initial, Stream, ExpectedVersion.NoStream));
        return (initial, ExpectedVersion.NoStream);
    }

    private async Task<IRoadNetworkView> ProcessPages(IRoadNetworkView view, ProcessSnapshotContext snapshotContext, ReadStreamPage page, ProcessMessageHandler processMessage, CancellationToken cancellationToken)
    {
        view = await ProcessPage(view, snapshotContext, page, processMessage, cancellationToken);
        
        var sw = Stopwatch.StartNew();
        while (!page.IsEnd && !snapshotContext.ProcessingCancelled)
        {
            sw.Restart();
            _logger.LogInformation("Next page read started with {Stream}, from version {FromStreamVersion} until {LastStreamVersion} and page size {StreamPageSize}", Stream, page.FromStreamVersion, page.LastStreamVersion, StreamPageSize);
            page = await page.ReadNext(cancellationToken);
            _logger.LogInformation("Next page read finished with {Stream}, from version {FromStreamVersion} until {LastStreamVersion} and page size {StreamPageSize} in {StopwatchElapsedMilliseconds}ms", Stream, page.FromStreamVersion, page.LastStreamVersion, StreamPageSize, sw.ElapsedMilliseconds);

            if (page.Status == PageReadStatus.StreamNotFound)
            {
                throw new InvalidOperationException($"Page status {page.Status} encountered while processing consecutive pages");
            }

            view = await ProcessPage(view, snapshotContext, page, processMessage, cancellationToken);
        }

        return view;
    }

    private async Task<IRoadNetworkView> ProcessPage(IRoadNetworkView view, ProcessSnapshotContext snapshotContext, ReadStreamPage page, ProcessMessageHandler processMessage, CancellationToken cancellationToken)
    {
        var messages = new List<object>(page.Messages.Length);
        
        foreach (var message in page.Messages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (processMessage != null && processMessage(message.StreamVersion, page.LastStreamVersion))
            {
                snapshotContext.ProcessingCancelled = true;
                _logger.LogInformation("Stopped processing more messages at StreamVersion {MessageStreamVersion}", message.StreamVersion);
                break;
            }

            messages.Add(
                JsonConvert.DeserializeObject(
                    await message.GetJsonData(cancellationToken),
                    _mapping.GetEventType(message.Type),
                    _settings));
            snapshotContext.Version = message.StreamVersion;
        }

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
            
            _logger.LogInformation("Read finished for RoadNetwork snapshot version {SnapshotVersion} in {StopwatchElapsedMilliseconds}ms", snapshotVersion, sw.ElapsedMilliseconds);

            if (snapshot != null && snapshotVersion != ExpectedVersion.NoStream)
            {
                version = snapshotVersion.Value;

                sw.Restart();
                _logger.LogInformation("View restore started from RoadNetwork snapshot version {SnapshotVersion}", version);
                view = view.RestoreFromSnapshot(snapshot);
                _logger.LogInformation("View restore finished for RoadNetwork snapshot version {SnapshotVersion} in {StopwatchElapsedMilliseconds}ms", version, sw.ElapsedMilliseconds);
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
