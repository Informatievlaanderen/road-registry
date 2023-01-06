namespace RoadRegistry.BackOffice.CommandHost;

using Abstractions;
using Framework;
using Hosts;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class RoadNetworkCommandProcessor : CommandProcessor
{
    public RoadNetworkCommandProcessor(
        IStreamStore streamStore,
        ICommandProcessorPositionStore positionStore,
        CommandHandlerDispatcher dispatcher,
        Scheduler scheduler,
        ILogger<RoadNetworkCommandProcessor> logger)
        : base(streamStore, RoadNetworkCommandQueue.Stream, positionStore, dispatcher, scheduler, RoadRegistryApplication.BackOffice, logger)
    {
    }
}
