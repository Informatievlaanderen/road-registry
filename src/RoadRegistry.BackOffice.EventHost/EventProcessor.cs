namespace RoadRegistry.BackOffice.EventHost;

using Framework;
using Hosts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class EventProcessor : PositionStoreEventProcessor
{
    private const string QueueName = WellKnownQueues.EventQueue;

    public EventProcessor(
        IHostApplicationLifetime hostApplicationLifetime,
        IStreamStore streamStore,
        IEventProcessorPositionStore positionStore,
        AcceptStreamMessageFilter filter,
        EventHandlerDispatcher dispatcher,
        Scheduler scheduler,
        ILoggerFactory loggerFactory)
        : base(hostApplicationLifetime, QueueName, streamStore, positionStore, filter, dispatcher, scheduler, loggerFactory)
    {
    }
}
