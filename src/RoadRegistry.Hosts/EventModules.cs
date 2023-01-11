namespace RoadRegistry.Hosts;

using System;
using BackOffice.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;

public static class EventModules
{
    public static RoadNetworkEventModule RoadNetwork(IServiceProvider sp)
    {
        return new RoadNetworkEventModule(
            sp.GetService<IStreamStore>(),
            sp.GetService<IRoadNetworkSnapshotReader>(),
            sp.GetService<IRoadNetworkSnapshotWriter>(),
            sp.GetService<IClock>(),
            sp.GetService<ILogger<RoadNetworkEventModule>>()
        );
    }
}
