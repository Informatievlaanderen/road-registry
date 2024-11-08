namespace RoadRegistry.BackOffice.ExtractHost;

using System;
using System.Threading;
using System.Threading.Tasks;
using Editor.Schema;
using Extensions;
using Framework;
using Hosts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class EventProcessor : PositionStoreEventProcessor
{
    private const string QueueName = WellKnownQueues.ExtractQueue;
    private const string EditorContextQueueName = WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost;

    private readonly IStreamStore _streamStore;
    private readonly Func<EditorContext> _editorContextFactory;

    public EventProcessor(
        IHostApplicationLifetime hostApplicationLifetime,
        IStreamStore streamStore,
        IEventProcessorPositionStore positionStore,
        AcceptStreamMessageFilter filter,
        EventHandlerDispatcher dispatcher,
        Scheduler scheduler,
        Func<EditorContext> editorContextFactory,
        ILoggerFactory loggerFactory)
        : base(hostApplicationLifetime, QueueName, streamStore, positionStore, filter, dispatcher, scheduler, loggerFactory)
    {
        _streamStore = streamStore;
        _editorContextFactory = editorContextFactory;
    }

    protected override async Task BeforeDispatchEvent(Event @event, CancellationToken cancellationToken)
    {
        await using var resumeContext = _editorContextFactory();
        await resumeContext.WaitForProjectionToBeAtStoreHeadPosition(_streamStore, EditorContextQueueName, Logger, cancellationToken);
    }
}
