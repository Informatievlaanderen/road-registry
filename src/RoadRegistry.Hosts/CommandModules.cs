namespace RoadRegistry.Hosts;

using System;
using Autofac;
using BackOffice.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;

public static class CommandModules
{
    public static RoadNetworkCommandModule RoadNetwork(IServiceProvider sp)
    {
        return new RoadNetworkCommandModule(
            sp.GetService<IStreamStore>(),
            sp.GetService<ILifetimeScope>(),
            sp.GetService<IRoadNetworkSnapshotReader>(),
            sp.GetService<IClock>(),
            sp.GetService<ILoggerFactory>()
        );
    }
}
