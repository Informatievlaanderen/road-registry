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
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

            _messagePumpCancellation = new CancellationTokenSource();
            _messageChannel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });
            _messagePump = Task.Run(async () =>
            {
                IAllStreamSubscription subscription = null;
                try
                {
                    while (await _messageChannel.Reader.WaitToReadAsync(_messagePumpCancellation.Token))
                    {
                        switch (await _messageChannel.Reader.ReadAsync(_messagePumpCancellation.Token))
                        {
                            case Resume _:
                                await using (var resumeContext = dbContextFactory())
                                {
                                    var projection = await resumeContext.ProjectionStates.SingleOrDefaultAsync(item => item.Name == RoadRegistryBackOfficeProjectionHost, _messagePumpCancellation.Token);
                                    long? after = projection?.Position ?? default;
                                    var head = await streamStore.ReadHeadPosition();
                                    if (head == Position.Start || (after.HasValue ? head - after <= CatchUpThreshold : head - CatchUpThreshold <= 0))
                                    {
                                        await _messageChannel.Writer.WriteAsync(new Subscribe(after), _messagePumpCancellation.Token);
                                    }
                                    else
                                    {
                                        await _messageChannel.Writer.WriteAsync(new CatchUp(after, CatchUpBatchSize), _messagePumpCancellation.Token);
                                    }
                                }

                                break;
                            case CatchUp catchUp:
                                var observedMessageCount = 0;
                                var catchUpPosition = catchUp.AfterPosition ?? Position.Start;
                                var context = dbContextFactory();
                                var page = await streamStore.ReadAllForwards(
                                    catchUpPosition,
                                    catchUp.BatchSize,
                                    true,
                                    _messagePumpCancellation.Token);

                                while (!page.IsEnd)
                                {
                                    foreach (var message in page.Messages)
                                    {
                                        if (catchUp.AfterPosition.HasValue &&
                                            message.Position == catchUp.AfterPosition.Value)
                                        {
                                            continue; // skip already processed message
                                        }

                                        if (filter(message))
                                        {
                                            var envelope = envelopeFactory.Create(message);
                                            var handlers = resolver(envelope);
                                            foreach (var handler in handlers)
                                            {
                                                await handler.Handler(context, envelope, _messagePumpCancellation.Token);
                                            }
                                        }

                                        observedMessageCount++;
                                        catchUpPosition = message.Position;

                                        if (observedMessageCount % CatchUpBatchSize == 0)
                                        {
                                            await context.UpdateProjectionState(
                                                RoadRegistryBackOfficeProjectionHost,
                                                catchUpPosition,
                                                _messagePumpCancellation.Token);
                                            await context.SaveChangesAsync(_messagePumpCancellation.Token);
                                            await context.DisposeAsync();

                                            context = dbContextFactory();
                                            observedMessageCount = 0;
                                        }
                                    }

                                    page = await page.ReadNext(_messagePumpCancellation.Token);
                                }

                                if (observedMessageCount > 0) // case where we just read the last page and pending work in memory needs to be flushed
                                {
                                    await context.UpdateProjectionState(
                                        RoadRegistryBackOfficeProjectionHost,
                                        catchUpPosition,
                                        _messagePumpCancellation.Token);
                                    await context.SaveChangesAsync(_messagePumpCancellation.Token);
                                    await context.DisposeAsync();
                                }

                                //switch to subscription as of the last page
                                await _messageChannel.Writer.WriteAsync(new Subscribe(catchUpPosition), _messagePumpCancellation.Token);
                                break;
                            case Subscribe _:
                                subscription?.Dispose();
                                await using (var subscribeContext = dbContextFactory())
                                {
                                    var projection = await subscribeContext.ProjectionStates.SingleOrDefaultAsync(
                                        item => item.Name == RoadRegistryBackOfficeProjectionHost,
                                        _messagePumpCancellation.Token);
                                    long? after = projection?.Position ?? default;
                                    subscription = streamStore.SubscribeToAll(
                                        after, async (_, message, token) =>
                                        {
                                            if (filter(message))
                                            {
                                                var command = new ProcessStreamMessage(message);
                                                await _messageChannel.Writer.WriteAsync(command, token);
                                                await command.Completion;
                                            }
                                            else if (message.Position % RecordPositionThreshold == 0 &&
                                                     !_messagePumpCancellation.IsCancellationRequested)
                                            {
                                                await _messageChannel.Writer.WriteAsync(new RecordPosition(message),
                                                    token);
                                            }
                                        }, async (_, reason, exception) =>
                                        {
                                            if (!_messagePumpCancellation.IsCancellationRequested)
                                            {
                                                await _messageChannel.Writer.WriteAsync(
                                                    new SubscriptionDropped(reason, exception),
                                                    _messagePumpCancellation.Token);
                                            }
                                        },
                                        prefetchJsonData: false,
                                        name: "RoadRegistry.BackOffice.Projector.EventProcessor");
                                }

                                break;
                            case RecordPosition record:
                                try
                                {
                                    logger.LogDebug("Recording position of stream message {MessageType} at position {Position} as processed.",
                                        record.Message.Type, record.Message.Position);

                                    await using (var recordContext = dbContextFactory())
                                    {
                                        await recordContext.UpdateProjectionState(RoadRegistryBackOfficeProjectionHost,
                                            record.Message.Position, _messagePumpCancellation.Token);
                                        await recordContext.SaveChangesAsync(_messagePumpCancellation.Token);
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
                                    logger.LogDebug("Processing stream message {MessageType} at position {Position}", process.Message.Type, process.Message.Position);

                                    var envelope = envelopeFactory.Create(process.Message);
                                    var handlers = resolver(envelope);
                                    await using (var processContext = dbContextFactory())
                                    {
                                        foreach (var handler in handlers)
                                        {
                                            await handler.Handler(processContext, envelope, _messagePumpCancellation.Token);
                                        }
                                        await processContext.UpdateProjectionState(
                                            RoadRegistryBackOfficeProjectionHost,
                                            process.Message.Position,
                                            _messagePumpCancellation.Token);
                                        await processContext.SaveChangesAsync(_messagePumpCancellation.Token);
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
                                    await scheduler.Schedule(async token =>
                                    {
                                        if (!_messagePumpCancellation.IsCancellationRequested)
                                        {
                                            await _messageChannel.Writer.WriteAsync(new Resume(), token);
                                        }
                                    }, ResubscribeAfter);
                                }
                                else if(dropped.Reason == SubscriptionDroppedReason.SubscriberError)
                                {
                                    logger.LogError(dropped.Exception, "Subscription was dropped because of a subscriber error.");

                                    if (dropped.Exception != null
                                        && dropped.Exception is SqlException sqlException
                                        && sqlException.Number == -2 /* timeout */)
                                    {
                                        await scheduler.Schedule(async token =>
                                        {
                                            if (!_messagePumpCancellation.IsCancellationRequested)
                                            {
                                                await _messageChannel.Writer.WriteAsync(new Resume(), token);
                                            }
                                        }, ResubscribeAfter);
                                    }
                                }

                                break;
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.Log(LogLevel.Information, "EventProcessor message pump is exiting due to cancellation.");
                    }
                }
                catch (OperationCanceledException)
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.Log(LogLevel.Information, "EventProcessor message pump is exiting due to cancellation.");
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "EventProcessor message pump is exiting due to a bug.");
                }
                finally
                {
                    subscription?.Dispose();
                }
            });
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
            await _scheduler.StartAsync(cancellationToken);
            await _messageChannel.Writer.WriteAsync(new Resume(), cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _messageChannel.Writer.Complete();
            _messagePumpCancellation.Cancel();
            await _messagePump;
            _messagePumpCancellation.Dispose();
            await _scheduler.StopAsync(cancellationToken);
        }
    }
}
