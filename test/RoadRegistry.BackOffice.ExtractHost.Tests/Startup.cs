namespace RoadRegistry.BackOffice.ExtractHost.Tests;

using Autofac;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Editor.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NetTopologySuite;
using NetTopologySuite.IO;

public class Startup : TestStartup
{
    protected override void ConfigureContainer(ContainerBuilder builder)
    {
    }

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        services
            .AddSingleton(new WKTReader(
                new NtsGeometryServices(
                    GeometryConfiguration.GeometryFactory.PrecisionModel,
                    GeometryConfiguration.GeometryFactory.SRID
                )))
            .AddSingleton(new RecyclableMemoryStreamManager())
            .AddSingleton<Func<EditorContext>>(sp =>
                () =>
                    new EditorContext(
                        new DbContextOptionsBuilder<EditorContext>()
                            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                            .UseSqlServer(
                                hostBuilderContext.Configuration.GetRequiredConnectionString(WellKnownConnectionNames.EditorProjections),
                                options => options
                                    .UseNetTopologySuite()
                            ).Options)
            );
    }
}
