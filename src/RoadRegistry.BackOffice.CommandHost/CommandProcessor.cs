namespace RoadRegistry.BackOffice.CommandHost;

using System;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Abstractions;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Hosts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;
using SqlStreamStore.Subscriptions;

public class CommandProcessor : RoadRegistryHostedService
{
    private static readonly TimeSpan ResubscribeAfter = TimeSpan.FromSeconds(5);

    private static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    private readonly Channel<object> _messageChannel;
    private readonly Task _messagePump;
    private readonly CancellationTokenSource _messagePumpCancellation;
    private readonly Scheduler _scheduler;
    private readonly RoadRegistryApplication _applicationProcessor;
    private readonly DistributedStreamStoreLock _distributedStreamStoreLock;

    public CommandProcessor(
        IHostApplicationLifetime hostApplicationLifetime,
        IStreamStore streamStore,
        StreamName queue,
        ICommandProcessorPositionStore positionStore,
        EventMapping commandMapping,
        CommandHandlerDispatcher dispatcher,
        Scheduler scheduler,
        RoadRegistryApplication applicationProcessor,
        DistributedStreamStoreLockOptions distributedStreamStoreLockOptions,
        ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(streamStore);
        ArgumentNullException.ThrowIfNull(positionStore);
        ArgumentNullException.ThrowIfNull(commandMapping);
        ArgumentNullException.ThrowIfNull(dispatcher);

        _scheduler = scheduler.ThrowIfNull();
        _applicationProcessor = applicationProcessor;
        _distributedStreamStoreLock = new DistributedStreamStoreLock(distributedStreamStoreLockOptions, queue, Logger);

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
                            case Subscribe:
                                Logger.LogInformation("Subscribing ...");
                                subscription?.Dispose();
                                var version = await positionStore
                                    .ReadVersion(queue.ToString(), _messagePumpCancellation.Token)
                                    .ConfigureAwait(false);
                                Logger.LogInformation("Subscribing as of {0}", version ?? -1);
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
                                    Logger.LogInformation(
                                        "Processing {MessageType} at {Position}",
                                        process.Message.Type, process.Message.Position);

                                    var messageMetadata = JsonConvert.DeserializeObject<MessageMetadata>(process.Message.JsonMetadata, SerializerSettings);
                                    var messageProcessor = messageMetadata?.Processor ?? RoadRegistryApplication.BackOffice;
                                    if (messageProcessor == _applicationProcessor)
                                    {
                                        await _distributedStreamStoreLock.RetryRunUntilLockAcquiredAsync(async () =>
                                        {
                                            var body = JsonConvert.DeserializeObject(
                                                await process.Message
                                                    .GetJsonData(_messagePumpCancellation.Token)
                                                    .ConfigureAwait(false),
                                                commandMapping.GetEventType(process.Message.Type),
                                                SerializerSettings);
                                            var command = new Command(body)
                                                .WithMessageId(process.Message.MessageId)
                                                .WithProvenanceData(messageMetadata?.ProvenanceData);

                                            await dispatcher(command, _messagePumpCancellation.Token).ConfigureAwait(false);
                                        }, _messagePumpCancellation.Token);
                                    }
                                    else
                                    {
                                        Logger.LogInformation("Skipping {MessageType} at {Position} - Message Processor '{MessageProcessor}' does not match '{Processor}'", process.Message.Type, process.Message.Position, messageProcessor, _applicationProcessor);
                                    }

                                    await positionStore
                                        .WriteVersion(queue.ToString(),
                                            process.Message.StreamVersion,
                                            _messagePumpCancellation.Token)
                                        .ConfigureAwait(false);
                                    process.Complete();

                                    Logger.LogInformation(
                                        "Processed {MessageType} at {Position}",
                                        process.Message.Type, process.Message.Position);
                                }
                                catch (Exception exception)
                                {
                                    Logger.LogError(exception, "Error while processing {MessageType} at {Position}",
                                        process.Message.Type, process.Message.Position);

                                    // how are we going to recover from this? do we even need to recover from this?
                                    // prediction: it's going to be a serialization error, a data quality error, or a bug
                                    process.Fault(exception);
                                    throw;
                                }

                                break;
                            case SubscriptionDropped dropped:
                                if (dropped.Reason == SubscriptionDroppedReason.StreamStoreError)
                                {
                                    Logger.LogError(dropped.Exception,
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
                                    Logger.LogError(dropped.Exception,
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
                Logger.LogInformation("CommandProcessor message pump is exiting due to task cancellation.");
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("CommandProcessor message pump is exiting due to operation cancellation.");
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "CommandProcessor message pump is exiting due to a bug.");
                hostApplicationLifetime.StopApplication();
            }
            finally
            {
                subscription?.Dispose();
            }
        }, _messagePumpCancellation.Token, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
    }

    protected override async Task StartingAsync(CancellationToken cancellationToken)
    {
        await _scheduler.StartAsync(cancellationToken).ConfigureAwait(false);
        await _messageChannel.Writer.WriteAsync(new Subscribe(), cancellationToken).ConfigureAwait(false);
    }

    protected override async Task StoppingAsync(CancellationToken cancellationToken)
    {
        _messageChannel.Writer.Complete();
        _messagePumpCancellation.Cancel();
        await _messagePump.ConfigureAwait(false);
        _messagePumpCancellation.Dispose();
        await _scheduler.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    private static bool CanResumeFrom(SubscriptionDropped dropped)
    {
        const int timeout = -2;
        return dropped.Exception is SqlException { Number: timeout } or IOException { InnerException: SqlException { Number: timeout } };
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
