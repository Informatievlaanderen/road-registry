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

    public class AtomFeedProcessor : IHostedService
    {
        private const string RoadRegistrySyndicationProjectionHost = "roadregistry-Syndication-projectionhost";

        private readonly Channel<object> _messageChannel;
        private readonly CancellationTokenSource _messagePumpCancellation;
        private readonly Task _messagePump;

        private readonly Scheduler _scheduler;
        private readonly ILogger<AtomFeedProcessor> _logger;

        private const int CatchUpBatchSize = 5000;

        public AtomFeedProcessor(
            IRegistryAtomFeedReader reader,
            // AcceptStreamMessageFilter filter,
            AtomEnvelopeFactory envelopeFactory,
            ConnectedProjectionHandlerResolver<SyndicationContext> resolver,
            Func<SyndicationContext> dbContextFactory,
            Scheduler scheduler,
            ILogger<AtomFeedProcessor> logger)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            // if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (envelopeFactory == null) throw new ArgumentNullException(nameof(envelopeFactory));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (dbContextFactory == null) throw new ArgumentNullException(nameof(dbContextFactory));

            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _messagePumpCancellation = new CancellationTokenSource();
            _messageChannel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });
            _messagePump = Task.Factory.StartNew(async () =>
            {
                try
                {
                    logger.LogInformation("EventProcessor message pump entered ...");
                    while (await _messageChannel.Reader.WaitToReadAsync(_messagePumpCancellation.Token).ConfigureAwait(false))
                    {
                        while (_messageChannel.Reader.TryRead(out var message))
                        {
                            switch (message)
                            {
                                case Resume _:
                                    logger.LogInformation("Resuming ...");
                                    await using (var resumeContext = dbContextFactory())
                                    {
                                        var projection =
                                            await resumeContext.ProjectionStates
                                                .SingleOrDefaultAsync(
                                                    item => item.Name == RoadRegistrySyndicationProjectionHost,
                                                    _messagePumpCancellation.Token)
                                                .ConfigureAwait(false);

                                        var after = projection?.Position;
                                        await _messageChannel.Writer
                                                .WriteAsync(new CatchUp(after, CatchUpBatchSize), _messagePumpCancellation.Token)
                                                .ConfigureAwait(false);
                                    }

                                    break;
                                case CatchUp catchUp:
                                    logger.LogInformation("Catching up as of {Position}", catchUp.AfterPosition ?? -1L);
                                    var observedMessageCount = 0;
                                    var catchUpPosition = catchUp.AfterPosition ?? Position.Start;
                                    var context = dbContextFactory();
                                    context.ChangeTracker.AutoDetectChangesEnabled = false;

                                    var entries = (await reader.ReadEntriesAsync(
                                            FeedUri,
                                            catchUpPosition,
                                            FeedUserName,
                                            FeedPassword,
                                            true,
                                            true))
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

                                                logger.LogInformation("Catching up on {MessageType} at {Position}",
                                                            atomEntry.ContentType, atomEntry.Id);

                                                var envelope = envelopeFactory.CreateEnvelope(atomEntry);
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
                                                    "Flushing catch up position of {0} and persisting changes ...",
                                                    catchUpPosition);
                                                await context
                                                    .UpdateProjectionState(
                                                        RoadRegistrySyndicationProjectionHost,
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
                                                FeedUri,
                                                catchUpPosition,
                                                FeedUserName,
                                                FeedPassword,
                                                true,
                                                true))
                                            .ToList();
                                    }

                                    if (observedMessageCount > 0) // case where we just read the last page and pending work in memory needs to be flushed
                                    {
                                        logger.LogInformation(
                                            "Flushing catch up position of {Position} and persisting changes ...",
                                            catchUpPosition);
                                        await context
                                            .UpdateProjectionState(
                                                RoadRegistrySyndicationProjectionHost,
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
                    // subscription?.Dispose();
                }
            }, _messagePumpCancellation.Token, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        public string FeedPassword => "";

        public string FeedUserName => "";

        public Uri FeedUri => new Uri("http://localhost:2002/v1/gemeenten/sync");

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
            _logger.LogInformation("Starting event processor ...");
            await _scheduler.StartAsync(cancellationToken).ConfigureAwait(false);
            await _messageChannel.Writer.WriteAsync(new Resume(), cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Started event processor.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping event processor ...");
            _messageChannel.Writer.Complete();
            _messagePumpCancellation.Cancel();
            await _messagePump.ConfigureAwait(false);
            _messagePumpCancellation.Dispose();
            await _scheduler.StopAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Stopped event processor.");
        }
    }
}
