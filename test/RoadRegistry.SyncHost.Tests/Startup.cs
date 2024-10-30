namespace RoadRegistry.SyncHost.Tests;

using Autofac;
using BackOffice.Extensions;
using BackOffice.Framework;
using Infrastructure.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sync.MunicipalityRegistry;
using Sync.StreetNameRegistry;

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

            .AddStreetNameConsumerServices()
            .AddInMemoryDbContextOptionsBuilder<StreetNameSnapshotConsumerContext>()

            .AddStreetNameProjectionServices()
            .AddInMemoryDbContextOptionsBuilder<StreetNameSnapshotProjectionContext>()
            .AddInMemoryDbContextOptionsBuilder<StreetNameEventProjectionContext>()

            .AddMunicipalityConsumerServices()
            .AddInMemoryDbContextOptionsBuilder<MunicipalityEventConsumerContext>()
            ;
    }
}
