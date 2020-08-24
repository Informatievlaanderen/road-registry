namespace RoadRegistry.Syndication.ProjectionHost
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Schema;
    using SqlStreamStore.Streams;

    public class AtomFeedProcessor<TConfiguration, TSyndicationContent> : IHostedService where TConfiguration : ISyndicationFeedConfiguration
    {
        private readonly Channel<object> _messageChannel;
        private readonly CancellationTokenSource _messagePumpCancellation;
        private readonly Task _messagePump;

        private readonly Scheduler _scheduler;
        private readonly ILogger<AtomFeedProcessor<TConfiguration, TSyndicationContent>> _logger;

        private const int CatchUpBatchSize = 5000;

        public AtomFeedProcessor(
            IRegistryAtomFeedReader reader,
            AtomEnvelopeFactory envelopeFactory,
            // AcceptStreamMessageFilter filter,
            ConnectedProjectionHandlerResolver<SyndicationContext> resolver,
            Func<SyndicationContext> dbContextFactory,
            Scheduler scheduler,
            TConfiguration feedConfiguration,
            ILogger<AtomFeedProcessor<TConfiguration, TSyndicationContent>> logger)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            // if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (envelopeFactory == null) throw new ArgumentNullException(nameof(envelopeFactory));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (dbContextFactory == null) throw new ArgumentNullException(nameof(dbContextFactory));
            if (feedConfiguration == null) throw new ArgumentNullException(nameof(feedConfiguration));

            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _messagePumpCancellation = new CancellationTokenSource();
            _messageChannel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });

            var roadRegistrySyndicationProjectionHost = $"roadregistry-syndication-projectionhost-{typeof(TSyndicationContent).Name}";

            _messagePump = Task.Factory.StartNew(async () =>
            {
                try
                {
                    logger.LogInformation("[{Context}] EventProcessor message pump entered ...", typeof(TSyndicationContent).Name);
                    while (await _messageChannel.Reader.WaitToReadAsync(_messagePumpCancellation.Token).ConfigureAwait(false))
                    {
                        while (_messageChannel.Reader.TryRead(out var message))
                        {
                            switch (message)
                            {
                                case Resume _:
                                    logger.LogInformation("[{Context}] Resuming ...", typeof(TSyndicationContent).Name);
                                    await using (var resumeContext = dbContextFactory())
                                    {
                                        var projection =
                                            await resumeContext.ProjectionStates
                                                .SingleOrDefaultAsync(
                                                    item => item.Name == roadRegistrySyndicationProjectionHost,
                                                    _messagePumpCancellation.Token)
                                                .ConfigureAwait(false);

                                        var after = projection?.Position;
                                        await _messageChannel.Writer
                                                .WriteAsync(new CatchUp(after, CatchUpBatchSize), _messagePumpCancellation.Token)
                                                .ConfigureAwait(false);
                                    }

                                    break;
                                case CatchUp catchUp:
                                    logger.LogInformation("[{Context}] Catching up as of {Position}", typeof(TSyndicationContent).Name, catchUp.AfterPosition ?? -1L);
                                    var observedMessageCount = 0;
                                    var catchUpPosition = catchUp.AfterPosition ?? Position.Start;
                                    var context = dbContextFactory();
                                    context.ChangeTracker.AutoDetectChangesEnabled = false;

                                    var entries = (await reader.ReadEntriesAsync(
                                            feedConfiguration.Uri,
                                            catchUpPosition,
                                            feedConfiguration.UserName,
                                            feedConfiguration.Password,
                                            true,
                                            false))
                                        .ToList();

                                    while (entries.Any())
                                    {
                                        if (!long.TryParse(entries.Last().Id, out var lastEntryId))
                                            break;

                                        foreach (var atomEntry in entries)
                                        {
                                            // if (catchUp.AfterPosition.HasValue &&
                                            //     streamMessage.Position == catchUp.AfterPosition.Value)
                                            // {
                                            //     continue; // skip already processed message
                                            // }

                                            // if (filter(streamMessage))
                                            if (true) // filter here?
                                            {

                                                logger.LogInformation("[{Context}] Catching up on {MessageType} at {Position}",
                                                            typeof(TSyndicationContent).Name, atomEntry.ContentType, atomEntry.Id);

                                                var envelope = envelopeFactory.CreateEnvelope<TSyndicationContent>(atomEntry);
                                                if (envelope != null)
                                                {
                                                    var handlers = resolver(envelope);
                                                    foreach (var handler in handlers)
                                                    {
                                                        await handler
                                                            .Handler(context, envelope, _messagePumpCancellation.Token)
                                                            .ConfigureAwait(false);
                                                    }
                                                }
                                            }

                                            observedMessageCount++;
                                            catchUpPosition = lastEntryId;

                                            if (observedMessageCount % CatchUpBatchSize == 0)
                                            {
                                                logger.LogInformation(
                                                    "[{Context}] Flushing catch up position of {Position} and persisting changes ...",
                                                    typeof(TSyndicationContent).Name, catchUpPosition);
                                                await context
                                                    .UpdateProjectionState(
                                                        roadRegistrySyndicationProjectionHost,
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

                                        catchUpPosition = lastEntryId + 1;
                                        entries = (await reader.ReadEntriesAsync(
                                                feedConfiguration.Uri,
                                                catchUpPosition,
                                                feedConfiguration.UserName,
                                                feedConfiguration.Password,
                                                true,
                                                false))
                                            .ToList();
                                    }

                                    if (observedMessageCount > 0) // case where we just read the last page and pending work in memory needs to be flushed
                                    {
                                        logger.LogInformation(
                                            "[{Context}] Flushing catch up position of {Position} and persisting changes ...",
                                            typeof(TSyndicationContent).Name, catchUpPosition);
                                        await context
                                            .UpdateProjectionState(
                                                roadRegistrySyndicationProjectionHost,
                                                catchUpPosition,
                                                _messagePumpCancellation.Token)
                                            .ConfigureAwait(false);
                                        context.ChangeTracker.DetectChanges();
                                        await context.SaveChangesAsync(_messagePumpCancellation.Token).ConfigureAwait(false);
                                    }

                                    await context.DisposeAsync().ConfigureAwait(false);

                                    //switch to subscription as of the last page
                                    // await _messageChannel.Writer
                                    //     .WriteAsync(
                                    //         new Subscribe(catchUpPosition),
                                    //         _messagePumpCancellation.Token)
                                    //     .ConfigureAwait(false);
                                    break;
                            }
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    logger.LogInformation("[{Context}] EventProcessor message pump is exiting due to cancellation", typeof(TSyndicationContent).Name);
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("[{Context}] EventProcessor message pump is exiting due to cancellation", typeof(TSyndicationContent).Name);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "[{Context}] EventProcessor message pump is exiting due to a bug", typeof(TSyndicationContent).Name);
                }
                finally
                {
                    // subscription?.Dispose();
                }
            }, _messagePumpCancellation.Token, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        private class Resume { }

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

        private class Subscribe
        {
            public Subscribe(long? afterPosition)
            {
                AfterPosition = afterPosition;
            }

            public long? AfterPosition { get; }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Context}] Starting event processor ...", typeof(TSyndicationContent).Name);
            await _scheduler.StartAsync(cancellationToken).ConfigureAwait(false);
            await _messageChannel.Writer.WriteAsync(new Resume(), cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("[{Context}] Started event processor.", typeof(TSyndicationContent).Name);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Context}] Stopping event processor ...", typeof(TSyndicationContent).Name);
            _messageChannel.Writer.Complete();
            _messagePumpCancellation.Cancel();
            await _messagePump.ConfigureAwait(false);
            _messagePumpCancellation.Dispose();
            await _scheduler.StopAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("[{Context}] Stopped event processor.", typeof(TSyndicationContent).Name);
        }
    }
}
