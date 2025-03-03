namespace RoadRegistry.Hosts;

using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;
using SqlStreamStore.Subscriptions;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public abstract class RunnerDbContextEventProcessor<TDbContext> : RoadRegistryHostedService
    where TDbContext : RunnerDbContext<TDbContext>
{
    private const int RecordPositionThreshold = 1;
    public static readonly EventMapping EventMapping = new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));
    private static readonly TimeSpan ResubscribeAfter = TimeSpan.FromSeconds(5);
    public static readonly JsonSerializerSettings SerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
    private readonly Channel<object> _messageChannel;
    private readonly Task _messagePump;
    private readonly CancellationTokenSource _messagePumpCancellation;
    private readonly Scheduler _scheduler;

    protected RunnerDbContextEventProcessor(
        string projectionStateName,
        IStreamStore streamStore,
        AcceptStreamMessage<TDbContext> acceptStreamMessage,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<TDbContext> resolver,
        IDbContextFactory<TDbContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        int? catchUpBatchSize = null,
        int? catchUpThreshold = null)
        : this(projectionStateName, streamStore, acceptStreamMessage.CreateFilter(), envelopeFactory, resolver, dbContextFactory.CreateDbContext, scheduler, loggerFactory, configuration,
            catchUpBatchSize, catchUpThreshold)
    {
    }

    protected RunnerDbContextEventProcessor(
        string projectionStateName,
        IStreamStore streamStore,
        AcceptStreamMessageFilter filter,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<TDbContext> resolver,
        IDbContextFactory<TDbContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        int? catchUpBatchSize = null,
        int? catchUpThreshold = null)
        : this(projectionStateName, streamStore, filter, envelopeFactory, resolver, dbContextFactory.CreateDbContext, scheduler, loggerFactory, configuration,
            catchUpBatchSize, catchUpThreshold)
    {
    }

    protected RunnerDbContextEventProcessor(
        string projectionStateName,
        IStreamStore streamStore,
        AcceptStreamMessageFilter filter,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<TDbContext> resolver,
        Func<TDbContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        int? catchUpBatchSize = null,
        int? catchUpThreshold = null)
        : base(loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(streamStore);
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(envelopeFactory);
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(dbContextFactory);
        ArgumentNullException.ThrowIfNull(scheduler);

        catchUpBatchSize ??= 500;
        var projectionStateNameInConfiguration = projectionStateName.Replace("-", "");
        var catchUpBatchSizeOverride = configuration.GetValue<int?>($"EventProcessorOptions:{projectionStateNameInConfiguration}:CatchUpBatchSizeOverride");
        if (catchUpBatchSizeOverride is not null)
        {
            catchUpBatchSize = catchUpBatchSizeOverride.Value;
            Logger.LogInformation("Overriding CatchUpBatchSize for projection '{StateName}' to {CatchUpBatchSize}", projectionStateName, catchUpBatchSize);
        }

        catchUpThreshold ??= 1000;
        var catchUpTresholdOverride = configuration.GetValue<int?>($"EventProcessorOptions:{projectionStateNameInConfiguration}:CatchUpTresholdOverride");
        if (catchUpTresholdOverride is not null)
        {
            catchUpThreshold = catchUpTresholdOverride.Value;
            Logger.LogInformation("Overriding CatchUpTreshold for projection '{StateName}' to {CatchUpThreshold}", projectionStateName, catchUpThreshold);
        }

        _scheduler = scheduler;

        _messagePumpCancellation = new CancellationTokenSource();
        _messageChannel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });
        _messagePump = Task.Factory.StartNew(async () =>
        {
            var sw = new Stopwatch();

            IAllStreamSubscription subscription = null;
            try
            {
                Logger.LogInformation("Message pump entered ...");
                while (await _messageChannel.Reader.WaitToReadAsync(_messagePumpCancellation.Token).ConfigureAwait(false))
                {
                    while (_messageChannel.Reader.TryRead(out var message))
                    {
                        switch (message)
                        {
                            case Resume:
                                Logger.LogInformation("Resuming ...");
                                await using (var resumeContext = dbContextFactory())
                                {
                                    var projection = await resumeContext.ProjectionStates
                                        .SingleOrDefaultAsync(
                                            item => item.Name == projectionStateName,
                                            _messagePumpCancellation.Token)
                                        .ConfigureAwait(false);
                                    var after = projection?.Position;
                                    var head = await streamStore.ReadHeadPosition();
                                    if (head == Position.Start
                                        || after.HasValue
                                            ? head - after.Value <= catchUpThreshold
                                            : head - catchUpThreshold <= 0)
                                    {
                                        await _messageChannel.Writer
                                            .WriteAsync(new Subscribe(after), _messagePumpCancellation.Token)
                                            .ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        await _messageChannel.Writer
                                            .WriteAsync(new CatchUp(after, catchUpBatchSize.Value), _messagePumpCancellation.Token)
                                            .ConfigureAwait(false);
                                    }
                                }

                                break;

                            case CatchUp catchUp:
                                Logger.LogInformation("Catching up as of {Position}", catchUp.AfterPosition ?? -1L);
                                sw.Restart();
                                var observedMessageCount = 0;
                                var catchUpPosition = catchUp.AfterPosition ?? Position.Start;
                                var context = dbContextFactory();
                                context.ChangeTracker.AutoDetectChangesEnabled = false;
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
                                            Logger.LogInformation("Catching up on {MessageType} at {Position}",
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

                                        if (observedMessageCount % catchUpBatchSize == 0)
                                        {
                                            Logger.LogInformation(
                                                "Flushing catch up position of {CatchUpPosition} and persisting changes ...",
                                                catchUpPosition);
                                            await context
                                                .UpdateProjectionState(
                                                    projectionStateName,
                                                    catchUpPosition,
                                                    _messagePumpCancellation.Token)
                                                .ConfigureAwait(false);
                                            await UpdateEventProcessorMetricsAsync(context, page.FromPosition, catchUpPosition, sw.ElapsedMilliseconds, _messagePumpCancellation.Token).ConfigureAwait(false);
                                            await OutputEstimatedTimeRemainingAsync(context, page.FromPosition - 1, await streamStore.ReadHeadPosition(), _messagePumpCancellation.Token).ConfigureAwait(false);
                                            sw.Restart();

                                            context.ChangeTracker.DetectChanges();
                                            await context.SaveChangesAsync(_messagePumpCancellation.Token).ConfigureAwait(false);
                                            await context.DisposeAsync().ConfigureAwait(false);

                                            context = dbContextFactory();
                                            context.ChangeTracker.AutoDetectChangesEnabled = false;
                                            observedMessageCount = 0;
                                        }
                                    }

                                    page = await page.ReadNext(_messagePumpCancellation.Token).ConfigureAwait(false);
                                }

                                if (observedMessageCount > 0) // case where we just read the last page and pending work in memory needs to be flushed
                                {
                                    Logger.LogInformation(
                                        "Flushing catch up position of {Position} and persisting changes ...",
                                        catchUpPosition);
                                    await context
                                        .UpdateProjectionState(
                                            projectionStateName,
                                            catchUpPosition,
                                            _messagePumpCancellation.Token)
                                        .ConfigureAwait(false);
                                    await UpdateEventProcessorMetricsAsync(context, page.FromPosition, catchUpPosition, sw.ElapsedMilliseconds, _messagePumpCancellation.Token).ConfigureAwait(false);
                                    await OutputEstimatedTimeRemainingAsync(context, page.FromPosition - 1, await streamStore.ReadHeadPosition(), _messagePumpCancellation.Token).ConfigureAwait(false);
                                    sw.Restart();

                                    context.ChangeTracker.DetectChanges();
                                    await context.SaveChangesAsync(_messagePumpCancellation.Token).ConfigureAwait(false);
                                }

                                await context.DisposeAsync().ConfigureAwait(false);
                                CatchUpCompleted?.Invoke(this, EventArgs.Empty);
                                //switch to subscription as of the last page
                                await _messageChannel.Writer
                                    .WriteAsync(
                                        new Subscribe(catchUpPosition),
                                        _messagePumpCancellation.Token)
                                    .ConfigureAwait(false);
                                break;

                            case Subscribe subscribe:
                                Logger.LogInformation("Subscribing as of {Position}", subscribe.AfterPosition ?? -1L);
                                subscription?.Dispose();
                                subscription = streamStore.SubscribeToAll(
                                    subscribe.AfterPosition, async (_, streamMessage, token) =>
                                    {
                                        if (filter(streamMessage))
                                        {
                                            Logger.LogInformation("Observing {MessageType} at {Position}",
                                                streamMessage.Type, streamMessage.Position);
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
                                    name: "RoadRegistry.Product.ProjectionHost.EventProcessor");

                                break;

                            case RecordPosition record:
                                try
                                {
                                    Logger.LogInformation("Recording position of {MessageType} at {Position}.",
                                        record.Message.Type, record.Message.Position);

                                    await using var recordContext = dbContextFactory();
                                    await recordContext
                                        .UpdateProjectionState(
                                            projectionStateName,
                                            record.Message.Position, _messagePumpCancellation.Token)
                                        .ConfigureAwait(false);
                                    await recordContext.SaveChangesAsync(_messagePumpCancellation.Token).ConfigureAwait(false);
                                }
                                catch (Exception exception)
                                {
                                    Logger.LogError(exception, "Error while recording position of {MessageType} at {Position}",
                                        record.Message.Type, record.Message.Position);
                                    throw;
                                }

                                break;

                            case ProcessStreamMessage process:
                                try
                                {
                                    Logger.LogInformation("Processing {MessageType} at {Position}",
                                        process.Message.Type, process.Message.Position);
                                    sw.Restart();

                                    var envelope = envelopeFactory.Create(process.Message);
                                    var handlers = resolver(envelope);
                                    await using (var processContext = dbContextFactory())
                                    {
                                        processContext.ChangeTracker.AutoDetectChangesEnabled = false;

                                        foreach (var handler in handlers)
                                        {
                                            await handler
                                                .Handler(processContext, envelope, _messagePumpCancellation.Token)
                                                .ConfigureAwait(false);
                                        }

                                        await processContext.UpdateProjectionState(
                                            projectionStateName,
                                            process.Message.Position,
                                            _messagePumpCancellation.Token).ConfigureAwait(false);
                                        await UpdateEventProcessorMetricsAsync(processContext, process.Message.Position, process.Message.Position, sw.ElapsedMilliseconds, _messagePumpCancellation.Token).ConfigureAwait(false);

                                        processContext.ChangeTracker.DetectChanges();
                                        await processContext.SaveChangesAsync(_messagePumpCancellation.Token).ConfigureAwait(false);
                                    }

                                    process.Complete();
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
                                    Logger.LogError(dropped.Exception,
                                        "Subscription was dropped because of a subscriber error");

                                    if (CanResumeFrom(dropped))
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
                Logger.LogInformation("Message pump is exiting due to task cancellation");
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("Message pump is exiting due to operation cancellation");
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "Message pump is exiting due to a bug");

                // This will mark the host as Unhealthy
                await StopAsync(_messagePumpCancellation.Token);
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
        await _messageChannel.Writer.WriteAsync(new Resume(), cancellationToken).ConfigureAwait(false);
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

    public event EventHandler CatchUpCompleted;

    protected virtual Task OutputEstimatedTimeRemainingAsync(TDbContext context, long currentPosition, long lastPosition, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected virtual Task UpdateEventProcessorMetricsAsync(TDbContext context, long fromPosition, long toPosition, long elapsedMilliseconds, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private sealed class CatchUp
    {
        public CatchUp(long? afterPosition, int batchSize)
        {
            AfterPosition = afterPosition;
            BatchSize = batchSize;
        }

        public long? AfterPosition { get; }
        public int BatchSize { get; }
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

    private sealed class Resume
    {
    }

    private sealed class Subscribe
    {
        public Subscribe(long? afterPosition)
        {
            AfterPosition = afterPosition;
        }

        public long? AfterPosition { get; }
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
