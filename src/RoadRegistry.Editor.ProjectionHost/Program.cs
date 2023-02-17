namespace RoadRegistry.Editor.ProjectionHost;

using BackOffice;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Hosts.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Newtonsoft.Json;
using Projections;
using Schema;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    private static readonly Encoding WindowsAnsiEncoding = Encoding.GetEncoding(1252);

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
                .AddSingleton(() =>
                    new EditorContext(
                        new DbContextOptionsBuilder<EditorContext>()
                            .UseSqlServer(
                                hostContext.Configuration.GetConnectionString(WellknownConnectionNames.EditorProjections),
                                options => options
                                    .EnableRetryOnFailure()
                                    .UseNetTopologySuite()
                            ).Options)
                )
                .AddSingleton(sp => new ConnectedProjection<EditorContext>[]
                {
                    new OrganizationRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), WindowsAnsiEncoding),
                    new MunicipalityGeometryProjection(),
                    new GradeSeparatedJunctionRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), WindowsAnsiEncoding),
                    new RoadNetworkChangeFeedProjection(sp.GetRequiredService<IBlobClient>()),
                    new RoadNetworkInfoProjection(),
                    new RoadNodeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), WindowsAnsiEncoding),
                    new RoadSegmentEuropeanRoadAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), WindowsAnsiEncoding),
                    new RoadSegmentLaneAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), WindowsAnsiEncoding),
                    new RoadSegmentNationalRoadAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), WindowsAnsiEncoding),
                    new RoadSegmentNumberedRoadAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), WindowsAnsiEncoding),
                    new RoadSegmentRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), WindowsAnsiEncoding),
                    new RoadSegmentSurfaceAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), WindowsAnsiEncoding),
                    new RoadSegmentWidthAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), WindowsAnsiEncoding),
                    new ExtractDownloadRecordProjection(),
                    new ExtractUploadRecordProjection()
                })
                .AddSingleton(sp => Resolve.WhenEqualToHandlerMessageType(sp
                    .GetRequiredService<ConnectedProjection<EditorContext>[]>()
                    .SelectMany(projection => projection.Handlers)
                        .ToArray()))
                .AddSingleton(sp => AcceptStreamMessage.WhenEqualToMessageType(sp.GetRequiredService<ConnectedProjection<EditorContext>[]>(), EventProcessor.EventMapping))
                .AddSingleton<IRunnerDbContextMigratorFactory>(new EditorContextMigrationFactory()))
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new []
            {
                WellknownConnectionNames.Events,
                WellknownConnectionNames.EditorProjections,
                WellknownConnectionNames.EditorProjectionsAdmin
            })
            .Log((sp, logger) =>
            {
                var blobClientOptions = sp.GetService<BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
            })
            .RunAsync(async (sp, host, configuration) =>
            {
                var loggerFactory = sp.GetService<ILoggerFactory>();
                var migratorFactory = sp.GetService<IRunnerDbContextMigratorFactory>();

                await migratorFactory.CreateMigrator(configuration, loggerFactory)
                    .MigrateAsync(CancellationToken.None).ConfigureAwait(false);
            });
    }
}
