namespace RoadRegistry.Wms.ProjectionHost;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Hosts.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Projections;
using Schema;
using SqlStreamStore;
using SqlStreamStore.Streams;

public class EventProcessor
{
    private const int CatchUpBatchSize = 5000;
    private const string QueueName = "roadregistry-wms-projectionhost";
    private const int SynchronizeWithCacheBatchSize = 5000;

    public static readonly EventMapping EventMapping =
        new(
            new List<IReadOnlyDictionary<string, Type>>
            {
                EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly),
                EventMapping.DiscoverEventNamesInAssembly(typeof(SynchronizeWithStreetNameCache).Assembly)
            }.SelectMany(dict => dict).ToDictionary(x => x.Key, x => x.Value));

    public static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    private readonly Func<WmsContext> _dbContextFactory;
    private readonly EnvelopeFactory _envelopeFactory;
    private readonly AcceptStreamMessageFilter _filter;
    private readonly ILogger<EventProcessor> _logger;
    private readonly IMetadataUpdater _metadataUpdater;
    private readonly ConnectedProjectionHandlerResolver<WmsContext> _resolver;
    private readonly IStreamStore _streamStore;
    private readonly IStreetNameCache _streetNameCache;

    public EventProcessor(
        IStreamStore streamStore,
        AcceptStreamMessageFilter filter,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<WmsContext> resolver,
        Func<WmsContext> dbContextFactory,
        IStreetNameCache streetNameCache,
        ILogger<EventProcessor> logger,
        IMetadataUpdater metadataUpdater)
    {
        _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
        _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        _envelopeFactory = envelopeFactory ?? throw new ArgumentNullException(nameof(envelopeFactory));
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _streetNameCache = streetNameCache ?? throw new ArgumentNullException(nameof(streetNameCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metadataUpdater = metadataUpdater ?? throw new ArgumentNullException(nameof(metadataUpdater));
    }

    private async Task CatchUp(long? after, int catchUpBatchSize, CancellationToken token)
    {
        _logger.LogInformation("Catching up as of {Position}", after ?? -1L);
        var observedMessageCount = 0;
        var catchUpPosition = after ?? Position.Start;
        var context = _dbContextFactory();
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        var page = await _streamStore
            .ReadAllForwards(
                catchUpPosition,
                catchUpBatchSize,
                true,
                token)
            .ConfigureAwait(false);

        while (page.Messages.Length > 0)
        {
            foreach (var streamMessage in page.Messages)
            {
                if (after.HasValue &&
                    streamMessage.Position == after.Value)
                    continue; // skip already processed message

                if (_filter(streamMessage))
                {
                    _logger.LogInformation("Catching up on {MessageType} at {Position}",
                        streamMessage.Type, streamMessage.Position);
                    var envelope = _envelopeFactory.Create(streamMessage);
                    var handlers = _resolver(envelope);
                    foreach (var handler in handlers)
                        await handler
                            .Handler(context, envelope, token)
                            .ConfigureAwait(false);
                }

                observedMessageCount++;
                catchUpPosition = streamMessage.Position;

                if (observedMessageCount % CatchUpBatchSize == 0)
                {
                    _logger.LogInformation(
                        "Flushing catch up position of {0} and persisting changes ...",
                        catchUpPosition);
                    await context
                        .UpdateProjectionState(
                            QueueName,
                            catchUpPosition,
                            token)
                        .ConfigureAwait(false);
                    context.ChangeTracker.DetectChanges();
                    await context.SaveChangesAsync(token).ConfigureAwait(false);
                    await context.DisposeAsync().ConfigureAwait(false);

                    context = _dbContextFactory();
                    context.ChangeTracker.AutoDetectChangesEnabled = false;
                    observedMessageCount = 0;
                }
            }

            page = await page.ReadNext(token).ConfigureAwait(false);
        }

        if (observedMessageCount > 0) // case where we just read the last page and pending work in memory needs to be flushed
        {
            _logger.LogInformation(
                "Flushing catch up position of {Position} and persisting changes ...",
                catchUpPosition);
            await context
                .UpdateProjectionState(
                    QueueName,
                    catchUpPosition,
                    token)
                .ConfigureAwait(false);
            context.ChangeTracker.DetectChanges();
            await context.SaveChangesAsync(token).ConfigureAwait(false);
        }

        await context.DisposeAsync().ConfigureAwait(false);
    }

    public async Task Resume(CancellationToken token)
    {
        _logger.LogInformation("Resuming ...");

        await using var resumeContext = _dbContextFactory();
        var projection =
            await resumeContext.ProjectionStates
                .SingleOrDefaultAsync(
                    item => item.Name == QueueName,
                    token)
                .ConfigureAwait(false);
        try
        {
            await CatchUp(projection?.Position, CatchUpBatchSize, token);
            await SynchronizeWithCache(token);
        }
        finally
        {
            await _metadataUpdater.UpdateAsync(token).ConfigureAwait(false);
        }
    }

    private async Task SynchronizeWithCache(CancellationToken token)
    {
        _logger.LogInformation("Syncing with street name cache ...");
        var syncWithCacheContext = _dbContextFactory();
        syncWithCacheContext.ChangeTracker.AutoDetectChangesEnabled = false;

        var roadSegmentMinStreetNameCachePosition =
            await syncWithCacheContext
                .RoadSegments
                .MinAsync(item => item.StreetNameCachePosition, token)
                .ConfigureAwait(false);

        var streetNameCacheMaxPosition =
            await _streetNameCache
                .GetMaxPositionAsync(token)
                .ConfigureAwait(false);

        var difference = streetNameCacheMaxPosition - roadSegmentMinStreetNameCachePosition;
        if (difference == 0)
        {
            _logger.LogInformation("No updates in street name cache. Skipping sync.");
            return;
        }

        _logger.LogInformation("Street name cache updated, synchronizing.");
        _logger.LogInformation("Street name cache difference: {@Difference}.",
            streetNameCacheMaxPosition - roadSegmentMinStreetNameCachePosition);

        while (difference > 0)
        {
            _logger.LogInformation("Street name records out of sync: {@OutOfSync}.",
                await syncWithCacheContext
                    .RoadSegments
                    .CountAsync(record => record.StreetNameCachePosition != streetNameCacheMaxPosition, token)
                    .ConfigureAwait(false));

            var envelope = new Envelope(new SynchronizeWithStreetNameCache
            {
                BatchSize = SynchronizeWithCacheBatchSize
            }, new Dictionary<string, object>()).ToGenericEnvelope();

            var handlers = _resolver(envelope);
            foreach (var handler in handlers)
                await handler.Handler(syncWithCacheContext, envelope, token)
                    .ConfigureAwait(false);

            syncWithCacheContext.ChangeTracker.DetectChanges();
            await syncWithCacheContext.SaveChangesAsync(token).ConfigureAwait(false);

            roadSegmentMinStreetNameCachePosition =
                await syncWithCacheContext
                    .RoadSegments
                    .MinAsync(item => item.StreetNameCachePosition, token)
                    .ConfigureAwait(false);

            await syncWithCacheContext.DisposeAsync().ConfigureAwait(false);
            streetNameCacheMaxPosition = await _streetNameCache.GetMaxPositionAsync(token).ConfigureAwait(false);
            difference = streetNameCacheMaxPosition - roadSegmentMinStreetNameCachePosition;

            syncWithCacheContext = _dbContextFactory();
            syncWithCacheContext.ChangeTracker.AutoDetectChangesEnabled = false;
        }

        _logger.LogInformation("No more updates in street name cache.");
        await syncWithCacheContext.DisposeAsync().ConfigureAwait(false);
    }
}