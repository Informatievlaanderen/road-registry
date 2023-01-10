namespace RoadRegistry.Hosts;

using System;
using BackOffice.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using BackOffice.Framework;
using SqlStreamStore;

public static class CommandModules
{
    public static RoadNetworkCommandModule RoadNetwork(IServiceProvider sp)
    {
        return new RoadNetworkCommandModule(
            sp.GetService<IStreamStore>(),
            sp.GetService<Func<EventSourcedEntityMap>>(),
            sp.GetService<IRoadNetworkSnapshotReader>(),
            sp.GetService<IRoadNetworkSnapshotWriter>(),
            sp.GetService<IClock>(),
            sp.GetService<ILogger<RoadNetworkCommandModule>>()
        );
    }
}
