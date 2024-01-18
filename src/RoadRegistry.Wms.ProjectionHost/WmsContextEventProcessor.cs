namespace RoadRegistry.Wms.ProjectionHost;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Abstractions;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Projections;
using Schema;
using SqlStreamStore;

public class WmsContextEventProcessor : DbContextEventProcessor<WmsContext>
{
    private const string QueueName = "roadregistry-wms-projectionhost";
    private const int CatchUpBatchSize = 5000;
    private const int SynchronizeWithCacheBatchSize = 5000;

    public WmsContextEventProcessor(
        IStreamStore streamStore,
        AcceptStreamMessage<WmsContext> filter,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<WmsContext> resolver,
        IDbContextFactory<WmsContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<WmsContextEventProcessor> logger,
        IStreetNameCache streetNameCache)
        : base(
            QueueName,
            streamStore,
            filter,
            envelopeFactory,
            resolver,
            dbContextFactory,
            scheduler,
            logger,
            catchUpBatchSize: CatchUpBatchSize,
            catchUpThreshold: 1000)
    {
        CatchUpCompleted += (o, e) =>
        {
            SynchronizeWithCache(
                    dbContextFactory,
                    resolver,
                    streetNameCache,
                    logger,
                    CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        };
    }

    private async Task SynchronizeWithCache(
        IDbContextFactory<WmsContext> dbContextFactory,
        ConnectedProjectionHandlerResolver<WmsContext> resolver,
        IStreetNameCache streetNameCache,
        ILogger<WmsContextEventProcessor> logger,
        CancellationToken token = default(CancellationToken))
    {
        //logger.LogInformation("Syncing with street name cache ...");
        //var syncWithCacheContext = await dbContextFactory.CreateDbContextAsync(token);
        //syncWithCacheContext.ChangeTracker.AutoDetectChangesEnabled = false;

        //var roadSegmentMinStreetNameCachePosition =
        //    await syncWithCacheContext
        //        .RoadSegments
        //        .MinAsync(item => item.StreetNameCachePosition, token)
        //        .ConfigureAwait(false);

        //var streetNameCacheMaxPosition =
        //    await streetNameCache
        //        .GetMaxPositionAsync(token)
        //        .ConfigureAwait(false);

        //var difference = streetNameCacheMaxPosition - roadSegmentMinStreetNameCachePosition;
        //if (difference == 0)
        //{
        //    logger.LogInformation("No updates in street name cache. Skipping sync.");
        //    return;
        //}

        //logger.LogInformation("Street name cache updated, synchronizing.");
        //logger.LogInformation("Street name cache difference: {@Difference}.",
        //    streetNameCacheMaxPosition - roadSegmentMinStreetNameCachePosition);

        //while (difference > 0)
        //{
        //    logger.LogInformation("Street name records out of sync: {@OutOfSync}.",
        //        await syncWithCacheContext
        //            .RoadSegments
        //            .CountAsync(record => record.StreetNameCachePosition != streetNameCacheMaxPosition, token)
        //            .ConfigureAwait(false));

        //    var envelope = new Envelope(new SynchronizeWithStreetNameCache
        //    {
        //        BatchSize = SynchronizeWithCacheBatchSize
        //    }, new Dictionary<string, object>()).ToGenericEnvelope();

        //    var handlers = resolver(envelope);
        //    foreach (var handler in handlers)
        //        await handler.Handler(syncWithCacheContext, envelope, token)
        //            .ConfigureAwait(false);

        //    syncWithCacheContext.ChangeTracker.DetectChanges();
        //    await syncWithCacheContext.SaveChangesAsync(token).ConfigureAwait(false);

        //    roadSegmentMinStreetNameCachePosition =
        //        await syncWithCacheContext
        //            .RoadSegments
        //            .MinAsync(item => item.StreetNameCachePosition, token)
        //            .ConfigureAwait(false);

        //    await syncWithCacheContext.DisposeAsync().ConfigureAwait(false);
        //    streetNameCacheMaxPosition = await streetNameCache.GetMaxPositionAsync(token).ConfigureAwait(false);
        //    difference = streetNameCacheMaxPosition - roadSegmentMinStreetNameCachePosition;

        //    syncWithCacheContext = await dbContextFactory.CreateDbContextAsync(token);
        //    syncWithCacheContext.ChangeTracker.AutoDetectChangesEnabled = false;
        //}

        //logger.LogInformation("No more updates in street name cache.");
        //await syncWithCacheContext.DisposeAsync().ConfigureAwait(false);
    }
}
