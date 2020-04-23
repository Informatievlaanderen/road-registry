namespace RoadRegistry.BackOffice.ProjectionHost
{
    using System;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Framework;
    using Messages;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Schema;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using SqlStreamStore.Subscriptions;

    public class EventProcessor : IHostedService
    {
        private const string RoadRegistryBackOfficeProjectionHost = "roadregistry-backoffice-projectionhost";

        public static readonly JsonSerializerSettings SerializerSettings =
            EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        public static readonly EventMapping EventMapping =
            new EventMapping(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

        private static readonly TimeSpan ResubscribeAfter = TimeSpan.FromSeconds(5);

        private readonly Channel<object> _messageChannel;
        private readonly CancellationTokenSource _messagePumpCancellation;
        private readonly Task _messagePump;

        private readonly Scheduler _scheduler;
        private readonly ILogger<EventProcessor> _logger;

        private const int CatchUpThreshold = 1000;
        private const int CatchUpBatchSize = 5000;
        private const int RecordPositionThreshold = 1000;

        public EventProcessor(
            IStreamStore streamStore,
            AcceptStreamMessageFilter filter,
            EnvelopeFactory envelopeFactory,
            ConnectedProjectionHandlerResolver<BackOfficeContext> resolver,
            Func<BackOfficeContext> dbContextFactory,
            Scheduler scheduler,
            ILogger<EventProcessor> logger)
        {
            if (streamStore == null) throw new ArgumentNullException(nameof(streamStore));
            if (filter == null) throw new ArgumentNullException(nameof(filter));
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
                IAllStreamSubscription subscription = null;
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
                                                    item => item.Name == RoadRegistryBackOfficeProjectionHost,
                                                    _messagePumpCancellation.Token)
                                                .ConfigureAwait(false);
                                        var after = projection?.Position;
                                        var head = await streamStore.ReadHeadPosition();
                                        if (head == Position.Start || (after.HasValue
                                            ? head - after.Value <= CatchUpThreshold
                                            : head - CatchUpThreshold <= 0))
                                        {
                                            await _messageChannel.Writer
                                                .WriteAsync(new Subscribe(after), _messagePumpCancellation.Token)
                                                .ConfigureAwait(false);
                                        }
                                        else
                                        {
                                            await _messageChannel.Writer
                                                .WriteAsync(new CatchUp(after, CatchUpBatchSize), _messagePumpCancellation.Token)
                                                .ConfigureAwait(false);
                                        }
                                    }

                                    break;
                                case CatchUp catchUp:
                                    logger.LogInformation("Catching up as of {Position}", catchUp.AfterPosition ?? -1L);
                                    var observedMessageCount = 0;
                                    var catchUpPosition = catchUp.AfterPosition ?? Position.Start;
                                    var context = dbContextFactory();
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
                                                        RoadRegistryBackOfficeProjectionHost,
                                                        catchUpPosition,
                                                        _messagePumpCancellation.Token)
                                                    .ConfigureAwait(false);
                                                await context.SaveChangesAsync(_messagePumpCancellation.Token).ConfigureAwait(false);
                                                await context.DisposeAsync().ConfigureAwait(false);

                                                context = dbContextFactory();
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
                                                RoadRegistryBackOfficeProjectionHost,
                                                catchUpPosition,
                                                _messagePumpCancellation.Token)
                                            .ConfigureAwait(false);
                                        await context.SaveChangesAsync(_messagePumpCancellation.Token).ConfigureAwait(false);
                                    }

                                    await context.DisposeAsync().ConfigureAwait(false);

                                    //switch to subscription as of the last page
                                    await _messageChannel.Writer
                                        .WriteAsync(
                                            new Subscribe(catchUpPosition),
                                            _messagePumpCancellation.Token)
                                        .ConfigureAwait(false);
                                    break;
                                case Subscribe subscribe:
                                    logger.LogInformation("Subscribing as of {0}", subscribe.AfterPosition ?? -1L);
                                    subscription?.Dispose();
                                    subscription = streamStore.SubscribeToAll(
                                        subscribe.AfterPosition, async (_, streamMessage, token) =>
                                        {
                                            if (filter(streamMessage))
                                            {
                                                logger.LogInformation("Observing {0} at {1}", streamMessage.Type,
                                                    streamMessage.Position);
                                                var command = new ProcessStreamMessage(streamMessage);
                                                await _messageChannel.Writer.WriteAsync(command, token).ConfigureAwait(false);
                                                await command.Completion.ConfigureAwait(false);
                                            }
                                            else if (streamMessage.Position % RecordPositionThreshold == 0 &&
                                                     !_messagePumpCancellation.IsCancellationRequested)
                                            {
                                                await _messageChannel.Writer
                                                    .WriteAsync(new RecordPosition(streamMessage), token)
                                                    .ConfigureAwait(false);
                                            }
                                        }, async (_, reason, exception) =>
                                        {
                                            if (!_messagePumpCancellation.IsCancellationRequested)
                                            {
                                                await _messageChannel.Writer
                                                    .WriteAsync(
                                                        new SubscriptionDropped(reason, exception),
                                                        _messagePumpCancellation.Token)
                                                    .ConfigureAwait(false);
                                            }
                                        },
                                        prefetchJsonData: false,
                                        name: "RoadRegistry.BackOffice.ProjectionHost.EventProcessor");

                                    break;
                                case RecordPosition record:
                                    try
                                    {
                                        logger.LogInformation("Recording position of {MessageType} at {Position}.",
                                            record.Message.Type, record.Message.Position);

                                        await using (var recordContext = dbContextFactory())
                                        {
                                            await recordContext
                                                .UpdateProjectionState(
                                                    RoadRegistryBackOfficeProjectionHost,
                                                    record.Message.Position, _messagePumpCancellation.Token)
                                                .ConfigureAwait(false);
                                            await recordContext.SaveChangesAsync(_messagePumpCancellation.Token).ConfigureAwait(false);
                                        }
                                    }
                                    catch (Exception exception)
                                    {
                                        logger.LogError(exception, exception.Message);
                                    }

                                    break;
                                case ProcessStreamMessage process:
                                    try
                                    {
                                        logger.LogInformation("Processing {MessageType} at {Position}",
                                            process.Message.Type, process.Message.Position);

                                        var envelope = envelopeFactory.Create(process.Message);
                                        var handlers = resolver(envelope);
                                        await using (var processContext = dbContextFactory())
                                        {
                                            foreach (var handler in handlers)
                                            {
                                                await handler
                                                    .Handler(processContext, envelope, _messagePumpCancellation.Token)
                                                    .ConfigureAwait(false);
                                            }

                                            await processContext.UpdateProjectionState(
                                                RoadRegistryBackOfficeProjectionHost,
                                                process.Message.Position,
                                                _messagePumpCancellation.Token).ConfigureAwait(false);
                                            await processContext.SaveChangesAsync(_messagePumpCancellation.Token).ConfigureAwait(false);
                                        }

                                        process.Complete();
                                    }
                                    catch (Exception exception)
                                    {
                                        logger.LogError(exception, exception.Message);

                                        // how are we going to recover from this? do we even need to recover from this?
                                        // prediction: it's going to be a serialization error, a data quality error, or a bug

                                        process.Fault(exception);
                                    }

                                    break;
                                case SubscriptionDropped dropped:
                                    if (dropped.Reason == SubscriptionDroppedReason.StreamStoreError)
                                    {
                                        logger.LogError(dropped.Exception,
                                            "Subscription was dropped because of a stream store error");
                                        await scheduler.Schedule(async token =>
                                        {
                                            if (!_messagePumpCancellation.IsCancellationRequested)
                                            {
                                                await _messageChannel.Writer.WriteAsync(new Resume(), token).ConfigureAwait(false);
                                            }
                                        }, ResubscribeAfter).ConfigureAwait(false);
                                    }
                                    else if (dropped.Reason == SubscriptionDroppedReason.SubscriberError)
                                    {
                                        logger.LogError(dropped.Exception,
                                            "Subscription was dropped because of a subscriber error");

                                        if (dropped.Exception != null
                                            && dropped.Exception is SqlException sqlException
                                            && sqlException.Number == -2 /* timeout */)
                                        {
                                            await scheduler.Schedule(async token =>
                                            {
                                                if (!_messagePumpCancellation.IsCancellationRequested)
                                                {
                                                    await _messageChannel.Writer.WriteAsync(new Resume(), token).ConfigureAwait(false);
                                                }
                                            }, ResubscribeAfter).ConfigureAwait(false);
                                        }
                                    }

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

        private class SubscriptionDropped
        {
            public SubscriptionDropped(SubscriptionDroppedReason reason, Exception exception)
            {
                Reason = reason;
                Exception = exception;
            }

            public SubscriptionDroppedReason Reason { get; }
            public Exception Exception { get; }
        }

        private class ProcessStreamMessage
        {
            private readonly TaskCompletionSource<object> _source;

            public ProcessStreamMessage(StreamMessage message)
            {
                Message = message;
                _source = new TaskCompletionSource<object>();
            }

            public StreamMessage Message { get; }

            public Task Completion => _source.Task;

            public void Complete() => _source.TrySetResult(null);
            public void Fault(Exception exception) => _source.TrySetException(exception);
        }

        private class RecordPosition
        {
            public StreamMessage Message { get; }

            public RecordPosition(StreamMessage message)
            {
                Message = message;
            }
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
