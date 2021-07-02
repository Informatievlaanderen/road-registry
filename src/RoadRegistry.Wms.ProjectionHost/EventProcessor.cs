namespace RoadRegistry.Wms.ProjectionHost
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Metadata;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Projections;
    using Schema;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public class EventProcessor : IHostedService
    {
        private const string RoadRegistryWmsProjectionHost = "roadregistry-wms-projectionhost";

        public static readonly JsonSerializerSettings SerializerSettings =
            EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        public static readonly EventMapping EventMapping =
            new EventMapping(
                new List<IReadOnlyDictionary<string, Type>>
                {
                    EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly),
                    EventMapping.DiscoverEventNamesInAssembly(typeof(SynchronizeWithStreetNameCache).Assembly)
                }.SelectMany(dict => dict).ToDictionary(x => x.Key, x => x.Value));

        private static readonly TimeSpan SynchronizeWithStreetNameCacheAfter = TimeSpan.FromMinutes(5);

        private readonly Channel<object> _messageChannel;
        private readonly CancellationTokenSource _messagePumpCancellation;
        private readonly Task _messagePump;

        private readonly Scheduler _scheduler;
        private readonly ILogger<EventProcessor> _logger;
        private readonly IHostApplicationLifetime _lifeTime;

        private const int CatchUpThreshold = 1000;
        private const int CatchUpBatchSize = 5000;
        private const int SynchronizeWithCacheBatchSize = 100;

        public EventProcessor(
            IStreamStore streamStore,
            AcceptStreamMessageFilter filter,
            EnvelopeFactory envelopeFactory,
            ConnectedProjectionHandlerResolver<WmsContext> resolver,
            Func<WmsContext> dbContextFactory,
            IStreetNameCache streetNameCache,
            Scheduler scheduler,
            ILogger<EventProcessor> logger,
            IHostApplicationLifetime lifeTime,
            IMetadataUpdater metadataUpdater)
        {
            if (streamStore == null) throw new ArgumentNullException(nameof(streamStore));
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (envelopeFactory == null) throw new ArgumentNullException(nameof(envelopeFactory));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (dbContextFactory == null) throw new ArgumentNullException(nameof(dbContextFactory));

            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lifeTime = lifeTime;

            _messagePumpCancellation = new CancellationTokenSource();
            _messageChannel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });
            _messagePump = Task.Factory.StartNew(async () =>
            {
                IAllStreamSubscription subscription = null;
                try
                {
                    logger.LogInformation("EventProcessor message pump entered ...");
                    while (!_messagePumpCancellation.IsCancellationRequested &&
                        await _messageChannel.Reader.WaitToReadAsync(_messagePumpCancellation.Token).ConfigureAwait(false))
                    {
                        while (_messageChannel.Reader.TryRead(out var message))
                        {
                            switch (message)
                            {
                                case SynchronizeWithCache _:
                                    logger.LogInformation("Syncing with cache ...");
                                    await using (var syncWithCacheContext = dbContextFactory())
                                    {
                                        var roadSegmentMinStreetNameCachePosition =
                                            await syncWithCacheContext.RoadSegments
                                                .MinAsync(
                                                    item => item.StreetNameCachePosition,
                                                    _messagePumpCancellation.Token)
                                                .ConfigureAwait(false);

                                        var streetNameCacheMaxPosition = await streetNameCache.GetMaxPositionAsync(_messagePumpCancellation.Token);

                                        if (streetNameCacheMaxPosition > roadSegmentMinStreetNameCachePosition)
                                        {
                                            logger.LogInformation("Street name cache difference: {@Difference}.", streetNameCacheMaxPosition - roadSegmentMinStreetNameCachePosition);
                                            logger.LogInformation("Street name records out of sync: {@OutOfSync}.", await syncWithCacheContext.RoadSegments.CountAsync(record => record.StreetNameCachePosition != streetNameCacheMaxPosition));
                                            logger.LogInformation("Street name cache updated, scheduling refresh.");

                                            var envelope = new Envelope(
                                                    new SynchronizeWithStreetNameCache
                                                    {
                                                        BatchSize = SynchronizeWithCacheBatchSize
                                                    },
                                                    new Dictionary<string, object>())
                                                .ToGenericEnvelope();

                                            var handlers = resolver(envelope);
                                            foreach (var handler in handlers)
                                            {
                                                await handler
                                                    .Handler(syncWithCacheContext, envelope, _messagePumpCancellation.Token)
                                                    .ConfigureAwait(false);
                                            }

                                            await syncWithCacheContext.SaveChangesAsync();
                                        }
                                        else
                                        {
                                            logger.LogInformation("No updates in street name cache, skipping refresh.");
                                        }
                                    }

                                    await scheduler.Schedule(async token =>
                                    {
                                        if (!_messagePumpCancellation.IsCancellationRequested)
                                        {
                                            await _messageChannel.Writer.WriteAsync(new SynchronizeWithCache(), token).ConfigureAwait(false);
                                        }
                                    }, SynchronizeWithStreetNameCacheAfter).ConfigureAwait(false);

                                    break;
                                case Resume _:
                                    logger.LogInformation("Resuming ...");
                                    await using (var resumeContext = dbContextFactory())
                                    {
                                        var projection =
                                            await resumeContext.ProjectionStates
                                                .SingleOrDefaultAsync(
                                                    item => item.Name == RoadRegistryWmsProjectionHost,
                                                    _messagePumpCancellation.Token)
                                                .ConfigureAwait(false);
                                        var after = projection?.Position;
                                        var head = await streamStore.ReadHeadPosition();
                                        if (head == Position.Start || (after.HasValue
                                            ? head - after.Value <= CatchUpThreshold
                                            : head - CatchUpThreshold <= 0))
                                        {
                                            await _messageChannel.Writer
                                                .WriteAsync(new CatchUp(after, CatchUpBatchSize), _messagePumpCancellation.Token)
                                                .ConfigureAwait(false);
                                        }
                                        else
                                        {
                                            await _messageChannel.Writer
                                                .WriteAsync(new CatchUp(after, CatchUpBatchSize), _messagePumpCancellation.Token)
                                                .ConfigureAwait(false);
                                        }

                                        await scheduler.Schedule(async token =>
                                        {
                                            if (!_messagePumpCancellation.IsCancellationRequested)
                                            {
                                                await _messageChannel.Writer.WriteAsync(new SynchronizeWithCache(), token).ConfigureAwait(false);
                                            }
                                        }, SynchronizeWithStreetNameCacheAfter).ConfigureAwait(false);
                                    }

                                    break;
                                case CatchUp catchUp:
                                    logger.LogInformation("Catching up as of {Position}", catchUp.AfterPosition ?? -1L);
                                    var observedMessageCount = 0;
                                    var catchUpPosition = catchUp.AfterPosition ?? Position.Start;
                                    var context = dbContextFactory();
                                    context.ChangeTracker.AutoDetectChangesEnabled = false;
                                    var page = await streamStore
                                        .ReadAllForwards(
                                            catchUpPosition,
                                            catchUp.BatchSize,
                                            true,
                                            _messagePumpCancellation.Token)
                                        .ConfigureAwait(false);

                                    while (!page.IsEnd)
                                    {
                                        foreach (var streamMessage in page.Messages)
                                        {
                                            if (catchUp.AfterPosition.HasValue &&
                                                streamMessage.Position == catchUp.AfterPosition.Value)
                                            {
                                                continue; // skip already processed message
                                            }

                                            if (filter(streamMessage))
                                            {
                                                logger.LogInformation("Catching up on {MessageType} at {Position}",
                                                    streamMessage.Type, streamMessage.Position);
                                                var envelope = envelopeFactory.Create(streamMessage);
                                                var handlers = resolver(envelope);
                                                foreach (var handler in handlers)
                                                {
                                                    await handler
                                                        .Handler(context, envelope, _messagePumpCancellation.Token)
                                                        .ConfigureAwait(false);
                                                }
                                            }

                                            observedMessageCount++;
                                            catchUpPosition = streamMessage.Position;

                                            if (observedMessageCount % CatchUpBatchSize == 0)
                                            {
                                                logger.LogInformation(
                                                    "Flushing catch up position of {0} and persisting changes ...",
                                                    catchUpPosition);
                                                await context
                                                    .UpdateProjectionState(
                                                        RoadRegistryWmsProjectionHost,
                                                        catchUpPosition,
                                                        _messagePumpCancellation.Token)
                                                    .ConfigureAwait(false);
                                                context.ChangeTracker.DetectChanges();
                                                await context.SaveChangesAsync(_messagePumpCancellation.Token).ConfigureAwait(false);
                                                await context.DisposeAsync().ConfigureAwait(false);

                                                context = dbContextFactory();
                                                context.ChangeTracker.AutoDetectChangesEnabled = false;
                                                observedMessageCount = 0;
                                            }
                                        }

                                        page = await page.ReadNext(_messagePumpCancellation.Token).ConfigureAwait(false);
                                    }

                                    if (observedMessageCount > 0) // case where we just read the last page and pending work in memory needs to be flushed
                                    {
                                        logger.LogInformation(
                                            "Flushing catch up position of {Position} and persisting changes ...",
                                            catchUpPosition);
                                        await context
                                            .UpdateProjectionState(
                                                RoadRegistryWmsProjectionHost,
                                                catchUpPosition,
                                                _messagePumpCancellation.Token)
                                            .ConfigureAwait(false);
                                        context.ChangeTracker.DetectChanges();
                                        await context.SaveChangesAsync(_messagePumpCancellation.Token).ConfigureAwait(false);
                                    }

                                    await context.DisposeAsync().ConfigureAwait(false);

                                    await metadataUpdater.UpdateAsync(_messagePumpCancellation.Token).ConfigureAwait(false);
                                    _lifeTime.StopApplication();

                                    break;
                            }
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    logger.LogInformation("EventProcessor message pump is exiting due to cancellation");
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("EventProcessor message pump is exiting due to cancellation");
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "EventProcessor message pump is exiting due to a bug");
                }
                finally
                {
                    subscription?.Dispose();
                }
            }, _messagePumpCancellation.Token, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        private class Resume { }

        private class SynchronizeWithCache { }

        private class CatchUp
        {
            public CatchUp(long? afterPosition, int batchSize)
            {
                AfterPosition = afterPosition;
                BatchSize = batchSize;
            }

            public long? AfterPosition { get; }
            public int BatchSize { get; }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting event processor ...");
            await _scheduler.StartAsync(cancellationToken).ConfigureAwait(false);
            await _messageChannel.Writer.WriteAsync(new Resume(), cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Started event processor.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping event processor ...");
            _messageChannel.Writer.Complete();
            await _messagePump.ConfigureAwait(false);
            _messagePumpCancellation.Cancel();
            _messagePumpCancellation.Dispose();
            await _scheduler.StopAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Stopped event processor.");
        }
    }
}
