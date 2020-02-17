namespace RoadRegistry.BackOffice.CommandHost
{
    using System;
    using System.Data.SqlClient;
    using System.Linq;
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

    public class CommandProcessor : IHostedService
    {
        private const string RoadNetworkCommandQueue = "roadnetwork-command-queue";

        private static readonly JsonSerializerSettings SerializerSettings =
            EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        private static readonly EventMapping CommandMapping =
            new EventMapping(RoadNetworkCommands.All.ToDictionary(command => command.Name));

        private readonly Scheduler _scheduler;
        private readonly ILogger<CommandProcessor> _logger;
        private static readonly TimeSpan ResubscribeAfter = TimeSpan.FromSeconds(5);

        private readonly BufferBlock<object> _messagePumpInbox;
        private readonly CancellationTokenSource _messagePumpCancellation;
        private readonly Task _messagePump;

        public CommandProcessor(
            IStreamStore streamStore,
            ICommandProcessorPositionStore positionStore,
            CommandHandlerDispatcher dispatcher,
            Scheduler scheduler,
            ILogger<CommandProcessor> logger)
        {
            if (streamStore == null) throw new ArgumentNullException(nameof(streamStore));
            if (positionStore == null) throw new ArgumentNullException(nameof(positionStore));
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));

            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
                IStreamSubscription subscription = null;
                try
                {
                    while (!_messagePumpCancellation.IsCancellationRequested)
                    {
                        switch (await _messagePumpInbox.ReceiveAsync(_messagePumpCancellation.Token))
                        {
                            case Subscribe _:
                                subscription?.Dispose();
                                var version = await positionStore.ReadVersion(RoadNetworkCommandQueue,
                                    _messagePumpCancellation.Token);
                                subscription = streamStore.SubscribeToStream(
                                    RoadNetworkCommandQueue,
                                    version,
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
                                    name: "RoadRegistry.BackOffice.CommandProcessor");
                                break;
                            case ProcessStreamMessage process:
                                try
                                {
                                    logger.LogDebug("Processing stream message {MessageType} at position {Position}", process.Message.Type, process.Message.Position);

                                    var body = JsonConvert.DeserializeObject(
                                        await process.Message.GetJsonData(_messagePumpCancellation.Token),
                                        CommandMapping.GetEventType(process.Message.Type),
                                        SerializerSettings);
                                    var command = new Command(body).WithMessageId(process.Message.MessageId);
                                    await dispatcher(command, _messagePumpCancellation.Token);
                                    await positionStore.WriteVersion(RoadNetworkCommandQueue,
                                        process.Message.StreamVersion,
                                        _messagePumpCancellation.Token);
                                    process.Complete();
                                }
                                catch (Exception exception)
                                {
                                    _logger.LogError(exception, exception.Message);

                                    // how are we going to recover from this? do we even need to recover from this?
                                    // prediction: it's going to be a serialization error, a data quality error, or a bug
//                                    if (process.Message.StreamVersion == 0)
//                                    {
//                                        await positionStore.WriteVersion(RoadNetworkCommandQueue,
//                                            process.Message.StreamVersion,
//                                            _messagePumpCancellation.Token);
//                                    }

                                    process.Fault(exception);
                                }

                                break;
                            case SubscriptionDropped dropped:
                                if (dropped.Reason == SubscriptionDroppedReason.StreamStoreError)
                                {
                                    _scheduler.Schedule(() =>
                                    {
                                        if (!_messagePumpCancellation.IsCancellationRequested)
                                        {
                                            _messagePumpInbox.Post(new Subscribe());
                                        }
                                    }, ResubscribeAfter);
                                }
                                else if (dropped.Reason == SubscriptionDroppedReason.SubscriberError)
                                {
                                    _logger.LogError(dropped.Exception,
                                        "Subscription was dropped because of a subscriber error.");

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
                    _logger.LogError(exception,
                        "CommandProcessor stopped processing due to an unexpected error.");
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
