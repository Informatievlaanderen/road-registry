namespace RoadRegistry.SyncHost.Tests;

using Autofac;
using BackOffice.Extensions;
using BackOffice.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoadRegistry.StreetNameConsumer.Schema;

public class Startup : TestStartup
{
    protected override void ConfigureContainer(HostBuilderContext hostContext, ContainerBuilder builder)
    {
    }

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        services
            .AddSingleton(new ApplicationMetadata(RoadRegistryApplication.BackOffice))
            .AddRoadNetworkCommandQueue()
            .AddDbContext<StreetNameConsumerContext>((sp, options) => options
                .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N")));
    }
}
