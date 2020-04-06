namespace RoadRegistry.BackOffice.EventHost
{
    using System;
    using Microsoft.Data.SqlClient;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Framework;
    using Messages;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using SqlStreamStore.Subscriptions;

    public class EventProcessor : IHostedService
    {
        private const string RoadNetworkArchiveEventQueue = "roadnetworkarchive-event-queue";

        private static readonly JsonSerializerSettings SerializerSettings =
            EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        public static readonly EventMapping EventMapping =
            new EventMapping(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

        private static readonly TimeSpan ResubscribeAfter = TimeSpan.FromSeconds(5);

        private readonly Channel<object> _messageChannel;
        private readonly CancellationTokenSource _messagePumpCancellation;
        private readonly Task _messagePump;

        private readonly Scheduler _scheduler;
        private readonly ILogger<EventProcessor> _logger;

        private const int RecordPositionThreshold = 1000;

        public EventProcessor(
            IStreamStore streamStore,
            IEventProcessorPositionStore positionStore,
            AcceptStreamMessageFilter filter,
            EventHandlerDispatcher dispatcher,
            Scheduler scheduler,
            ILogger<EventProcessor> logger)
        {
            if (streamStore == null) throw new ArgumentNullException(nameof(streamStore));
            if (positionStore == null) throw new ArgumentNullException(nameof(positionStore));
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));

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
                                case Subscribe _:
                                    logger.LogInformation("Subscribing ...");
                                    subscription?.Dispose();
                                    var position = await positionStore
                                        .ReadPosition(RoadNetworkArchiveEventQueue, _messagePumpCancellation.Token)
                                        .ConfigureAwait(false);
                                    logger.LogInformation("Subscribing as of {0}", position);
                                    subscription = streamStore.SubscribeToAll(
                                        position, async (_, streamMessage, token) =>
                                        {
                                            if (filter(streamMessage))
                                            {
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
                                            else
                                            {
                                                logger.LogInformation("Skipping {MessageType} at {Position}", streamMessage.Type, streamMessage.Position);
                                            }
                                        }, async (_, reason, exception) =>
                                        {
                                            if (!_messagePumpCancellation.IsCancellationRequested)
                                            {
                                                await _messageChannel.Writer
                                                    .WriteAsync(new SubscriptionDropped(reason, exception), _messagePumpCancellation.Token)
                                                    .ConfigureAwait(false);
                                            }
                                        },
                                        prefetchJsonData: false,
                                        name: "RoadRegistry.BackOffice.EventHost.EventProcessor");
                                    break;
                                case RecordPosition record:
                                    try
                                    {
                                        logger.LogInformation("Recording position of {MessageType} at {Position}.",
                                            record.Message.Type, record.Message.Position);

                                        await positionStore
                                            .WritePosition(
                                                RoadNetworkArchiveEventQueue,
                                                record.Message.Position,
                                                _messagePumpCancellation.Token)
                                            .ConfigureAwait(false);
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

                                        var body = JsonConvert.DeserializeObject(
                                            await process.Message.GetJsonData(_messagePumpCancellation.Token).ConfigureAwait(false),
                                            EventMapping.GetEventType(process.Message.Type),
                                            SerializerSettings);
                                        var @event = new Event(body).WithMessageId(process.Message.MessageId);
                                        await dispatcher(@event, _messagePumpCancellation.Token).ConfigureAwait(false);

                                        await positionStore
                                            .WritePosition(
                                                RoadNetworkArchiveEventQueue,
                                                process.Message.Position,
                                                _messagePumpCancellation.Token)
                                            .ConfigureAwait(false);
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
                                                await _messageChannel.Writer.WriteAsync(new Subscribe(), token).ConfigureAwait(false);
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
                                                    await _messageChannel.Writer.WriteAsync(new Subscribe(), token).ConfigureAwait(false);
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
            }, _messagePumpCancellation.Token, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        private class Subscribe { }

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
            await _messageChannel.Writer.WriteAsync(new Subscribe(), cancellationToken).ConfigureAwait(false);
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
