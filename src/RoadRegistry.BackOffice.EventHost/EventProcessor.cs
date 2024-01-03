namespace RoadRegistry.BackOffice.EventHost;

using Framework;
using Hosts;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class EventProcessor : PositionStoreEventProcessor<SqlEventProcessorPositionStore>
{
    private const string QueueName = WellknownQueues.EventQueue;

    public EventProcessor(
        IStreamStore streamStore,
        IEventProcessorPositionStore positionStore,
        AcceptStreamMessageFilter filter,
        EventHandlerDispatcher dispatcher,
        Scheduler scheduler,
        ILogger<EventProcessor> logger)
        : base(QueueName, streamStore, positionStore, filter, dispatcher, scheduler, logger)
    {
    }
}
