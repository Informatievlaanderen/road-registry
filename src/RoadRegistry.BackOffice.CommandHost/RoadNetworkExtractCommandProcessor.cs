namespace RoadRegistry.BackOffice.CommandHost;

using Abstractions;
using Framework;
using Hosts;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class RoadNetworkExtractCommandProcessor : CommandProcessor
{
    public RoadNetworkExtractCommandProcessor(
        IStreamStore streamStore,
        ICommandProcessorPositionStore positionStore,
        CommandHandlerDispatcher dispatcher,
        Scheduler scheduler,
        ILogger<RoadNetworkExtractCommandProcessor> logger)
        : base(streamStore, RoadNetworkExtractCommandQueue.Stream, positionStore, dispatcher, scheduler, logger)
    {
    }
}