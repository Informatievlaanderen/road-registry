namespace RoadRegistry.StreetNameConsumer.ProjectionHost.Tests;

using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Schema;

public class Startup : TestStartup
{
    protected override void ConfigureContainer(HostBuilderContext hostContext, ContainerBuilder builder)
    {
    }

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        services
            .AddDbContext<StreetNameConsumerContext>((sp, options) => options
                .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N")));
    }
}
