namespace RoadRegistry.BackOffice.ExtractHost;

using System;
using System.Threading;
using System.Threading.Tasks;
using Editor.Schema;
using Framework;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class EventProcessor : PositionStoreEventProcessor<SqlEventProcessorPositionStore>
{
    private const string QueueName = WellknownQueues.ExtractQueue;
    private const string EditorContextQueueName = WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost;

    private readonly IStreamStore _streamStore;
    private readonly Func<EditorContext> _editorContextFactory;

    public EventProcessor(
        IStreamStore streamStore,
        IEventProcessorPositionStore positionStore,
        AcceptStreamMessageFilter filter,
        EventHandlerDispatcher dispatcher,
        Scheduler scheduler,
        Func<EditorContext> editorContextFactory,
        ILogger<EventProcessor> logger)
        : base(QueueName, streamStore, positionStore, filter, dispatcher, scheduler, logger)
    {
        _streamStore = streamStore;
        _editorContextFactory = editorContextFactory;
    }

    protected override async Task BeforeDispatchEvent(Event @event, CancellationToken cancellationToken)
    {
        var loggedWaitingMessage = false;

        await using (var resumeContext = _editorContextFactory())
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var projection = await resumeContext.ProjectionStates
                    .SingleOrDefaultAsync(item => item.Name == EditorContextQueueName, cancellationToken)
                    .ConfigureAwait(false);
                var projectionPosition = projection?.Position;
                var headPosition = await _streamStore.ReadHeadPosition(cancellationToken);

                var editorProjectionIsUpToDate = projectionPosition == headPosition;
                if (editorProjectionIsUpToDate)
                {
                    if (loggedWaitingMessage)
                    {
                        Logger.LogInformation("{DbContext} projection queue {EditorContextQueueName} is up-to-date", resumeContext.GetType().Name, EditorContextQueueName);
                    }
                    return;
                }

                if (!loggedWaitingMessage)
                {
                    Logger.LogInformation("Waiting for {DbContext} projection queue {EditorContextQueueName} to be up-to-date ...", resumeContext.GetType().Name, EditorContextQueueName);
                    loggedWaitingMessage = true;
                }

                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}
