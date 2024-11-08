namespace RoadRegistry.BackOffice.CommandHost;

using Abstractions;
using Framework;
using Hosts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

//TODO-rik aparte processor voor system healthcheck?
// concreet: nieuwe stream "healthcheck" met eigen processor, de checks moeten dan ook in die stream worden geregistreerd
public class RoadNetworkCommandProcessor : CommandProcessor
{
    public RoadNetworkCommandProcessor(
        IHostApplicationLifetime hostApplicationLifetime,
        IStreamStore streamStore,
        ICommandProcessorPositionStore positionStore,
        CommandHandlerDispatcher dispatcher,
        Scheduler scheduler,
        DistributedStreamStoreLockOptions distributedStreamStoreLockOptions,
        ILoggerFactory loggerFactory)
        : base(hostApplicationLifetime, streamStore, RoadNetworkCommandQueue.Stream, positionStore, dispatcher, scheduler, RoadRegistryApplication.BackOffice, distributedStreamStoreLockOptions, loggerFactory)
    {
    }
}
