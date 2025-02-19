namespace RoadRegistry.Product.ProjectionHost;

using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using EventProcessors;
using Extensions;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Newtonsoft.Json;
using Projections;
using Schema;

public class Program
{
    public const int HostingPort = 10005;

    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = CreateHostBuilder(args).Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings([
                WellKnownConnectionNames.Events,
                WellKnownConnectionNames.ProductProjections,
                WellKnownConnectionNames.ProductProjectionsAdmin
            ])
            .RunAsync(async (sp, host, configuration) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var migratorFactory = sp.GetRequiredService<IRunnerDbContextMigratorFactory>();

                await migratorFactory.CreateMigrator(configuration, loggerFactory)
                    .MigrateAsync(CancellationToken.None).ConfigureAwait(false);
            });
    }

    public static RoadRegistryHostBuilder<Program> CreateHostBuilder(string[] args) => new RoadRegistryHostBuilder<Program>(args)
        .ConfigureServices((hostContext, services) =>
        {
            services
                .AddSingleton(new EnvelopeFactory(
                    ProductContextEventProcessor.EventMapping,
                    new EventDeserializer((eventData, eventType) =>
                        JsonConvert.DeserializeObject(eventData, eventType, ProductContextEventProcessor.SerializerSettings)))
                )
                .AddSingleton(() =>
                    new ProductContext(
                        new DbContextOptionsBuilder<ProductContext>()
                            .UseSqlServer(
                                hostContext.Configuration.GetRequiredConnectionString(WellKnownConnectionNames.ProductProjections),
                                options => options
                                    .EnableRetryOnFailure()
                                    .UseNetTopologySuite()
                            ).Options)
                )
                .AddSingleton<IRunnerDbContextMigratorFactory>(new ProductContextMigrationFactory())
                .AddProductContextEventProcessor<RoadNetworkEventProcessor>(sp =>
                [
                    new RoadNetworkInfoProjection(),
                    new RoadNodeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                    new RoadSegmentRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>(), sp.GetRequiredService<ILogger<RoadSegmentRecordProjection>>()),
                    new RoadSegmentEuropeanRoadAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                    new RoadSegmentLaneAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                    new RoadSegmentNationalRoadAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                    new RoadSegmentNumberedRoadAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                    new RoadSegmentSurfaceAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                    new RoadSegmentWidthAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                    new GradeSeparatedJunctionRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>())
                ])
                .AddProductContextEventProcessor<OrganizationV2EventProcessor>(sp =>
                [
                    new OrganizationRecordV2Projection()
                ])
                ;
        })
        .ConfigureHealthChecks(HostingPort, builder => builder
            .AddHostedServicesStatus()
        )
    ;
}
