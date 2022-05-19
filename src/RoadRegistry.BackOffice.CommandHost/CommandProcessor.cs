namespace RoadRegistry.BackOffice.CommandHost
{
    using System;
    using System.IO;
    using Microsoft.Data.SqlClient;
    using System.Linq;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Framework;
    using Hosts;
    using Messages;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using SqlStreamStore.Subscriptions;

    public class CommandProcessor : IHostedService
    {
        private static readonly JsonSerializerSettings SerializerSettings =
            EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        private static readonly EventMapping CommandMapping =
            new EventMapping(RoadNetworkCommands.All.ToDictionary(command => command.Name));

        private readonly Scheduler _scheduler;
        private readonly ILogger<CommandProcessor> _logger;
        private static readonly TimeSpan ResubscribeAfter = TimeSpan.FromSeconds(5);

        private readonly Channel<object> _messageChannel;
        private readonly CancellationTokenSource _messagePumpCancellation;
        private readonly Task _messagePump;

        public CommandProcessor(
            IStreamStore streamStore,
            StreamName queue,
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
            _messageChannel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = true
            });
            _messagePump = Task.Factory.StartNew(async () =>
            {
                IStreamSubscription subscription = null;
                try
                {
                    while (await _messageChannel.Reader.WaitToReadAsync(_messagePumpCancellation.Token).ConfigureAwait(false))
                    {
                        while (_messageChannel.Reader.TryRead(out var message))
                        {
                            switch (message)
                            {
                                case Subscribe _:
                                    logger.LogInformation("Subscribing ...");
                                    subscription?.Dispose();
                                    var version = await positionStore
                                        .ReadVersion(queue.ToString(), _messagePumpCancellation.Token)
                                        .ConfigureAwait(false);
                                    logger.LogInformation("Subscribing as of {0}", version ?? -1);
                                    subscription = streamStore.SubscribeToStream(
                                        queue.ToString(),
                                        version,
                                        async (_, streamMessage, token) =>
                                        {
                                            var command = new ProcessStreamMessage(streamMessage);
                                            await _messageChannel.Writer
                                                .WriteAsync(command, token)
                                                .ConfigureAwait(false);
                                            await command
                                                .Completion
                                                .ConfigureAwait(false);
                                        },
                                        async (_, reason, exception) =>
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
                                        name: "RoadRegistry.BackOffice.CommandHost.CommandProcessor");
                                    break;
                                case ProcessStreamMessage process:
                                    try
                                    {
                                        logger.LogDebug(
                                            "Processing {MessageType} at {Position}",
                                            process.Message.Type, process.Message.Position);

                                        var body = JsonConvert.DeserializeObject(
                                            await process.Message
                                                .GetJsonData(_messagePumpCancellation.Token)
                                                .ConfigureAwait(false),
                                            CommandMapping.GetEventType(process.Message.Type),
                                            SerializerSettings);
                                        var command = new Command(body).WithMessageId(process.Message.MessageId);
                                        await dispatcher(command, _messagePumpCancellation.Token).ConfigureAwait(false);
                                        await positionStore
                                            .WriteVersion(queue.ToString(),
                                                process.Message.StreamVersion,
                                                _messagePumpCancellation.Token)
                                            .ConfigureAwait(false);
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
                                        logger.LogError(dropped.Exception,
                                            "Subscription was dropped because of a stream store error.");
                                        await scheduler.Schedule(async token =>
                                        {
                                            if (!_messagePumpCancellation.IsCancellationRequested)
                                            {
                                                await _messageChannel.Writer.WriteAsync(new Subscribe(), token).ConfigureAwait(false);
                                            }
                                        }, ResubscribeAfter);
                                    }
                                    else if (dropped.Reason == SubscriptionDroppedReason.SubscriberError)
                                    {
                                        logger.LogError(dropped.Exception,
                                            "Subscription was dropped because of a subscriber error.");

                                        if (CanResumeFrom(dropped))
                                        {
                                            await scheduler.Schedule(async token =>
                                            {
                                                if (!_messagePumpCancellation.IsCancellationRequested)
                                                {
                                                    await _messageChannel.Writer.WriteAsync(new Subscribe(), token).ConfigureAwait(false);
                                                }
                                            }, ResubscribeAfter);
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
                        logger.Log(LogLevel.Information, "CommandProcessor message pump is exiting due to cancellation.");
                    }
                }
                catch (OperationCanceledException)
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.Log(LogLevel.Information, "CommandProcessor message pump is exiting due to cancellation.");
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "CommandProcessor message pump is exiting due to a bug.");
                }
                finally
                {
                    subscription?.Dispose();
                }
            }, _messagePumpCancellation.Token, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        private static bool CanResumeFrom(SubscriptionDropped dropped)
        {
            const int timeout = -2;
            return dropped.Exception != null
                   && (dropped.Exception is SqlException { Number: timeout } ||
                       dropped.Exception is IOException { InnerException: SqlException { Number: timeout } });
        }

        private sealed class Subscribe { }

        private sealed class SubscriptionDropped
        {
            public SubscriptionDropped(SubscriptionDroppedReason reason, Exception exception)
            {
                Reason = reason;
                Exception = exception;
            }

            public SubscriptionDroppedReason Reason { get; }
            public Exception Exception { get; }
        }

        private sealed class ProcessStreamMessage
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
            _logger.LogInformation("Starting command processor ...");
            await _scheduler.StartAsync(cancellationToken).ConfigureAwait(false);
            await _messageChannel.Writer.WriteAsync(new Subscribe(), cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Started command processor.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping command processor ...");
            _messageChannel.Writer.Complete();
            _messagePumpCancellation.Cancel();
            await _messagePump.ConfigureAwait(false);
            _messagePumpCancellation.Dispose();
            await _scheduler.StopAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Stopped command processor.");
        }
    }
}
