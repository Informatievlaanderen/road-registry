namespace RoadRegistry.Hosts;

using System;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BackOffice.Framework;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;
using SqlStreamStore.Subscriptions;

public abstract class PositionStoreEventProcessor<TEventProcessorPositionStore> : IHostedService
    where TEventProcessorPositionStore : IEventProcessorPositionStore
{
    private const int RecordPositionThreshold = 1;
    public static readonly EventMapping EventMapping = new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));
    private static readonly TimeSpan ResubscribeAfter = TimeSpan.FromSeconds(5);
    private static readonly JsonSerializerSettings SerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
    private readonly Channel<object> _messageChannel;
    private readonly Task _messagePump;
    private readonly CancellationTokenSource _messagePumpCancellation;
    private readonly Scheduler _scheduler;

    protected PositionStoreEventProcessor(
        string queueName,
        IStreamStore streamStore,
        IEventProcessorPositionStore positionStore,
        AcceptStreamMessageFilter filter,
        EventHandlerDispatcher dispatcher,
        Scheduler scheduler,
        ILogger<PositionStoreEventProcessor<TEventProcessorPositionStore>> logger)
    {
        ArgumentNullException.ThrowIfNull(streamStore);
        ArgumentNullException.ThrowIfNull(positionStore);
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(logger);

        _scheduler = scheduler;
        Logger = logger;

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
                        await ProcessMessage(queueName, streamStore, positionStore, filter, dispatcher, scheduler, logger, message, new Ref<IAllStreamSubscription>(subscription));
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
                throw;
            }
            finally
            {
                subscription?.Dispose();
            }
        }, _messagePumpCancellation.Token, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
    }

    protected ILogger<PositionStoreEventProcessor<TEventProcessorPositionStore>> Logger { get; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Starting event processor ...");
        await _scheduler.StartAsync(cancellationToken).ConfigureAwait(false);
        await _messageChannel.Writer.WriteAsync(new Subscribe(), cancellationToken).ConfigureAwait(false);
        Logger.LogInformation("Started event processor.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Stopping event processor ...");
        _messageChannel.Writer.Complete();
        _messagePumpCancellation.Cancel();
        await _messagePump.ConfigureAwait(false);
        _messagePumpCancellation.Dispose();
        await _scheduler.StopAsync(cancellationToken).ConfigureAwait(false);
        Logger.LogInformation("Stopped event processor.");
    }

    protected virtual Task BeforeDispatchEvent(Event @event, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static bool CanResumeFrom(SubscriptionDropped dropped)
    {
        const int timeout = -2;
        return dropped.Exception is SqlException { Number: timeout } or IOException { InnerException: SqlException { Number: timeout } };
    }

    private async Task ProcessMessage(
        string queueName,
        IStreamStore streamStore,
        IEventProcessorPositionStore positionStore,
        AcceptStreamMessageFilter filter,
        EventHandlerDispatcher dispatcher,
        Scheduler scheduler,
        ILogger<PositionStoreEventProcessor<TEventProcessorPositionStore>> logger,
        object message,
        Ref<IAllStreamSubscription> subscription)
    {
        switch (message)
        {
            case Subscribe:
                logger.LogInformation("Subscribing ...");
                subscription.Value?.Dispose();
                var position = await positionStore
                    .ReadPosition(queueName, _messagePumpCancellation.Token)
                    .ConfigureAwait(false);
                logger.LogInformation("Subscribing as of {0}", position ?? -1L);
                subscription.Value = streamStore.SubscribeToAll(
                    position, async (_, streamMessage, token) =>
                    {
                        if (filter(streamMessage))
                        {
                            var command = new ProcessStreamMessage(streamMessage);
                            await _messageChannel.Writer.WriteAsync(command, token).ConfigureAwait(false);
                            await command.Completion.ConfigureAwait(false);
                        }
                        else if (streamMessage.Position % RecordPositionThreshold == 0 && !_messagePumpCancellation.IsCancellationRequested)
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
                    name: GetType().FullName);
                break;

            case RecordPosition record:
                try
                {
                    logger.LogInformation("Recording position of {MessageType} at {Position}.",
                        record.Message.Type, record.Message.Position);

                    await positionStore
                        .WritePosition(
                            queueName,
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
                    var @event = new Event(body)
                        .WithMessageId(process.Message.MessageId)
                        .WithStream(process.Message.StreamId, process.Message.StreamVersion);

                    await BeforeDispatchEvent(@event, _messagePumpCancellation.Token).ConfigureAwait(false);
                    _messagePumpCancellation.Token.ThrowIfCancellationRequested();

                    await dispatcher(@event, _messagePumpCancellation.Token).ConfigureAwait(false);
                    await positionStore
                        .WritePosition(
                            queueName,
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
                        }, ResubscribeAfter)
                        .ConfigureAwait(false);
                }
                else if (dropped.Reason == SubscriptionDroppedReason.SubscriberError)
                {
                    logger.LogError(dropped.Exception,
                        "Subscription was dropped because of a subscriber error");

                    if (CanResumeFrom(dropped))
                    {
                        await scheduler.Schedule(async token =>
                            {
                                if (!_messagePumpCancellation.IsCancellationRequested)
                                {
                                    await _messageChannel.Writer.WriteAsync(new Subscribe(), token).ConfigureAwait(false);
                                }
                            }, ResubscribeAfter)
                            .ConfigureAwait(false);
                    }
                }

                break;
        }
    }

    private sealed class ProcessStreamMessage
    {
        private readonly TaskCompletionSource<object> _source;

        public ProcessStreamMessage(StreamMessage message)
        {
            Message = message;
            _source = new TaskCompletionSource<object>();
        }

        public Task Completion => _source.Task;
        public StreamMessage Message { get; }

        public void Complete()
        {
            _source.TrySetResult(null);
        }

        public void Fault(Exception exception)
        {
            _source.TrySetException(exception);
        }
    }

    private sealed class RecordPosition
    {
        public RecordPosition(StreamMessage message)
        {
            Message = message;
        }

        public StreamMessage Message { get; }
    }

    public sealed class Ref<T>
    {
        public Ref(T value)
        {
            Value = value;
        }

        public T Value { get; set; }
    }

    private sealed class Subscribe
    {
    }

    private sealed class SubscriptionDropped
    {
        public SubscriptionDropped(SubscriptionDroppedReason reason, Exception exception)
        {
            Reason = reason;
            Exception = exception;
        }

        public Exception Exception { get; }
        public SubscriptionDroppedReason Reason { get; }
    }
}
