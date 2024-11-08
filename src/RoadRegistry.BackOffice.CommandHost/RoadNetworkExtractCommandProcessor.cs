namespace RoadRegistry.BackOffice.CommandHost;

using Abstractions;
using Framework;
using Hosts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class RoadNetworkExtractCommandProcessor : CommandProcessor
{
    public RoadNetworkExtractCommandProcessor(
        IHostApplicationLifetime hostApplicationLifetime,
        IStreamStore streamStore,
        ICommandProcessorPositionStore positionStore,
        CommandHandlerDispatcher dispatcher,
        Scheduler scheduler,
        DistributedStreamStoreLockOptions distributedStreamStoreLockOptions,
        ILoggerFactory loggerFactory)
        : base(hostApplicationLifetime, streamStore, RoadNetworkExtractCommandQueue.Stream, positionStore, dispatcher, scheduler, RoadRegistryApplication.BackOffice, distributedStreamStoreLockOptions, loggerFactory)
    {
    }
}
