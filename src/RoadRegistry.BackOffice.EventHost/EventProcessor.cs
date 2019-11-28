namespace RoadRegistry.BackOffice.EventHost
{
    using System;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
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
        private static readonly EventMapping EventMapping =
            new EventMapping(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

        private static readonly TimeSpan ResubscribeAfter = TimeSpan.FromSeconds(5);

        private readonly BufferBlock<object> _messagePumpInbox;
        private readonly CancellationTokenSource _messagePumpCancellation;
        private readonly Task _messagePump;

        private readonly Scheduler _scheduler;

        public EventProcessor(
            IStreamStore streamStore,
            IEventProcessorPositionStore positionStore,
            EventHandlerDispatcher dispatcher,
            Scheduler scheduler,
            ILogger<EventProcessor> logger)
        {
            if (streamStore == null) throw new ArgumentNullException(nameof(streamStore));
            if (positionStore == null) throw new ArgumentNullException(nameof(positionStore));
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

            _messagePumpCancellation = new CancellationTokenSource();
            _messagePumpInbox = new BufferBlock<object>(new DataflowBlockOptions
            {
                BoundedCapacity = int.MaxValue,
                EnsureOrdered = true,
                MaxMessagesPerTask = 1,
                CancellationToken = _messagePumpCancellation.Token
            });
            _messagePump = Task.Run(async () =>
            {
                IAllStreamSubscription subscription = null;
                try
                {
                    while (!_messagePumpCancellation.IsCancellationRequested)
                    {
                        switch (await _messagePumpInbox.ReceiveAsync(_messagePumpCancellation.Token))
                        {
                            case Subscribe _:
                                subscription?.Dispose();
                                var position = await positionStore.ReadPosition(RoadNetworkArchiveEventQueue, _messagePumpCancellation.Token);
                                //if position + threshold < head position then catchup
                                subscription = streamStore.SubscribeToAll(
                                    position,
                                    (_, message, token) =>
                                    {
                                        var command = new ProcessStreamMessage(message);
                                        _messagePumpInbox.Post(command);
                                        return command.Completion;
                                    },
                                    (_, reason, exception) =>
                                    {
                                        if (!_messagePumpCancellation.IsCancellationRequested)
                                        {
                                            _messagePumpInbox.Post(new SubscriptionDropped(reason, exception));
                                        }
                                    },
                                    prefetchJsonData: false,
                                    name: "RoadRegistry.BackOffice.EventProcessor");
                                break;
                            case ProcessStreamMessage process:
                                try
                                {
                                    logger.LogDebug("Processing stream message {MessageType} at position {Position}", process.Message.Type, process.Message.Position);

                                    if (EventMapping.HasEventType(process.Message.Type))
                                    {
                                        var body = JsonConvert.DeserializeObject(
                                            await process.Message.GetJsonData(_messagePumpCancellation.Token),
                                            EventMapping.GetEventType(process.Message.Type),
                                            SerializerSettings);
                                        var @event = new Event(body).WithMessageId(process.Message.MessageId);
                                        await dispatcher(@event, _messagePumpCancellation.Token);
                                    }

                                    await positionStore.WritePosition(RoadNetworkArchiveEventQueue, process.Message.Position,
                                        _messagePumpCancellation.Token);
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
                                    scheduler.Schedule(() =>
                                    {
                                        if (!_messagePumpCancellation.IsCancellationRequested)
                                        {
                                            _messagePumpInbox.Post(new Subscribe());
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
                                        scheduler.Schedule(() =>
                                        {
                                            if (!_messagePumpCancellation.IsCancellationRequested)
                                            {
                                                _messagePumpInbox.Post(new Subscribe());
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
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception exception)
                {
                    logger.LogError(exception,
                        "EventProcessor stopped processing due to an unexpected error.");
                }
                finally
                {
                    subscription?.Dispose();
                }
            });
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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _scheduler.StartAsync(cancellationToken);
            await _messagePumpInbox.SendAsync(new Subscribe(), cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _messagePumpCancellation.Cancel();
            _messagePumpInbox.Complete();
            await _messagePumpInbox.Completion;
            await _messagePump;
            _messagePumpCancellation.Dispose();
            await _scheduler.StopAsync(cancellationToken);
        }
    }
}
