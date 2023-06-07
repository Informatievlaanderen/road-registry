namespace RoadRegistry.Product.ProjectionHost;

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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) => services
                .AddHostedService<EventProcessor>()
                .AddSingleton(new EnvelopeFactory(
                    EventProcessor.EventMapping,
                    new EventDeserializer((eventData, eventType) =>
                        JsonConvert.DeserializeObject(eventData, eventType, EventProcessor.SerializerSettings)))
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
                .AddSingleton(sp => AcceptStreamMessage.WhenEqualToMessageType(sp.GetRequiredService<ConnectedProjection<ProductContext>[]>(), EventProcessor.EventMapping))
                .AddSingleton<IRunnerDbContextMigratorFactory>(new ProductContextMigrationFactory()))
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new []
            {
                WellknownConnectionNames.Events,
                WellknownConnectionNames.ProductProjections,
                WellknownConnectionNames.ProductProjectionsAdmin
            })
            .RunAsync(async (sp, host, configuration) =>
            {
                var migratorFactory = sp.GetRequiredService<IRunnerDbContextMigratorFactory>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                await migratorFactory.CreateMigrator(configuration, loggerFactory)
                    .MigrateAsync(CancellationToken.None).ConfigureAwait(false);
            });
    }
}
