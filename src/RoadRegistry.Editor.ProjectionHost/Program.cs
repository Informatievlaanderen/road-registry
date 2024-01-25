namespace RoadRegistry.Editor.ProjectionHost;

using System.Linq;
using Autofac;
using BackOffice;
using BackOffice.Configuration;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
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
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Extensions;
using BackOffice.FeatureToggles;

public class Program
{
    public const int HostingPort = 10004;

    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = CreateHostBuilder(args).Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new[]
            {
                WellKnownConnectionNames.Events,
                WellKnownConnectionNames.EditorProjections,
                WellKnownConnectionNames.EditorProjectionsAdmin
            })
            .Log((sp, logger) =>
            {
                var blobClientOptions = sp.GetRequiredService<BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
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
        .ConfigureServices((hostContext, services) =>
        {
            var featureToggles = hostContext.Configuration.GetFeatureToggles<ApplicationFeatureToggle>();
            var useExtractRequestOverlapEventProcessorFeatureToggle = featureToggles.OfType<UseExtractRequestOverlapEventProcessorFeatureToggle>().Single();

            services
                .AddSingleton(new EnvelopeFactory(
                    EditorContextEventProcessor.EventMapping,
                    new EventDeserializer((eventData, eventType) =>
                        JsonConvert.DeserializeObject(eventData, eventType, EditorContextEventProcessor.SerializerSettings)))
                )
                .AddSingleton(() =>
                    new EditorContext(
                        new DbContextOptionsBuilder<EditorContext>()
                            .UseSqlServer(
                                hostContext.Configuration.GetConnectionString(WellKnownConnectionNames.EditorProjections),
                                options => options
                                    .EnableRetryOnFailure()
                                    .UseNetTopologySuite()
                            ).Options)
                )
                .AddSingleton<IRunnerDbContextMigratorFactory>(new EditorContextMigrationFactory())
                .AddEditorContextEventProcessor<RoadNetworkEventProcessor>(sp => new ConnectedProjection<EditorContext>[]
                {
                    new RoadNetworkInfoProjection(),
                    new GradeSeparatedJunctionRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                    new RoadNodeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                    new RoadSegmentEuropeanRoadAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                    new RoadSegmentLaneAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                    new RoadSegmentNationalRoadAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                    new RoadSegmentNumberedRoadAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                    new RoadSegmentRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                    new RoadSegmentSurfaceAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>()),
                    new RoadSegmentWidthAttributeRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>())
                })
                .AddEditorContextEventProcessor<OrganizationEventProcessor>(sp => new ConnectedProjection<EditorContext>[]
                {
                    new OrganizationRecordProjection(sp.GetRequiredService<RecyclableMemoryStreamManager>(), sp.GetRequiredService<FileEncoding>())
                })
                .AddEditorContextEventProcessor<MunicipalityEventProcessor>(sp => new ConnectedProjection<EditorContext>[]
                {
                    new MunicipalityGeometryProjection()
                })
                .AddEditorContextEventProcessor<ChangeFeedEventProcessor>(sp => new ConnectedProjection<EditorContext>[]
                {
                    new RoadNetworkChangeFeedProjection(sp.GetRequiredService<IBlobClient>())
                })
                .AddEditorContextEventProcessor<ExtractDownloadEventProcessor>(sp => new ConnectedProjection<EditorContext>[]
                {
                    new ExtractDownloadRecordProjection()
                })
                .AddEditorContextEventProcessor<ExtractRequestEventProcessor>(sp => new ConnectedProjection<EditorContext>[]
                {
                    new ExtractRequestRecordProjection(),
                    !useExtractRequestOverlapEventProcessorFeatureToggle.FeatureEnabled ? new ExtractRequestOverlapRecordProjection(sp.GetRequiredService<ILogger<ExtractRequestOverlapRecordProjection>>()) : null
                }.Where(x => x is not null).ToArray())
                .AddEditorContextEventProcessor<ExtractUploadEventProcessor>(sp => new ConnectedProjection<EditorContext>[]
                {
                    new ExtractUploadRecordProjection()
                });

            if (useExtractRequestOverlapEventProcessorFeatureToggle.FeatureEnabled)
            {
                services
                    .AddEditorContextEventProcessor<ExtractRequestOverlapEventProcessor>(sp => new ConnectedProjection<EditorContext>[]
                    {
                        new ExtractRequestOverlapRecordProjection(sp.GetRequiredService<ILogger<ExtractRequestOverlapRecordProjection>>())
                    });
            }
        })
        .ConfigureHealthChecks(HostingPort, builder => builder
            .AddSqlServer()
            .AddHostedServicesStatus()
            .AddS3(x => x
                .CheckPermission(WellKnownBuckets.UploadsBucket, Permission.Read)
            )
        )
        .ConfigureContainer((context, builder) =>
        {
            builder
                .Register(c => c.Resolve<RoadNetworkUploadsBlobClient>())
                .As<IBlobClient>().SingleInstance();
        });
}
