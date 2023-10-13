namespace RoadRegistry.Product.ProjectionHost;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
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
    public const int HostingPort = 10016;

    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = CreateHostBuilder(args).Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new[]
            {
                WellknownConnectionNames.Events,
                WellknownConnectionNames.ProductProjections,
                WellknownConnectionNames.ProductProjectionsAdmin
            })
            .RunAsync(async (sp, host, configuration) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var migratorFactory = sp.GetRequiredService<IRunnerDbContextMigratorFactory>();

                await migratorFactory.CreateMigrator(configuration, loggerFactory)
                    .MigrateAsync(CancellationToken.None).ConfigureAwait(false);
            });
    }

    public static RoadRegistryHostBuilder<Program> CreateHostBuilder(string[] args) => new RoadRegistryHostBuilder<Program>(args)
        .ConfigureServices((hostContext, services) => services
            .AddHostedService<ProductContextEventProcessor>()
            .AddSingleton(new EnvelopeFactory(
                ProductContextEventProcessor.EventMapping,
                new EventDeserializer((eventData, eventType) =>
                    JsonConvert.DeserializeObject(eventData, eventType, ProductContextEventProcessor.SerializerSettings)))
            )
            .AddSingleton(
                () =>
                    new ProductContext(
                        new DbContextOptionsBuilder<ProductContext>()
                            .UseSqlServer(
                                hostContext.Configuration.GetConnectionString(WellknownConnectionNames.ProductProjections),
                                options => options.EnableRetryOnFailure()
                            ).Options)
            )
            .AddSingleton<IRunnerDbContextMigratorFactory>(new ProductContextMigrationFactory())
            .AddSingleton(sp => new ConnectedProjection<ProductContext>[]
            {
                new OrganizationRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                new GradeSeparatedJunctionRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                new RoadNetworkInfoProjection(),
                new RoadNodeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                new RoadSegmentEuropeanRoadAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                new RoadSegmentLaneAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                new RoadSegmentNationalRoadAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                new RoadSegmentNumberedRoadAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                new RoadSegmentRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                new RoadSegmentSurfaceAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                new RoadSegmentWidthAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>())
            })
            .AddSingleton(sp => Resolve.WhenEqualToHandlerMessageType(sp
                .GetRequiredService<ConnectedProjection<ProductContext>[]>()
                .SelectMany(projection => projection.Handlers)
                .ToArray())
            )
            .AddSingleton(sp => AcceptStreamMessage.WhenEqualToMessageType(sp.GetRequiredService<ConnectedProjection<ProductContext>[]>(), ProductContextEventProcessor.EventMapping))
        )
        .ConfigureHealthChecks(HostingPort, builder => builder
            .AddSqlServer()
            .AddS3(x => x
                .CheckPermission(WellknownBuckets.UploadsBucket, Permission.Read)
            )
        )
    ;
}
