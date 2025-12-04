namespace RoadRegistry.Editor.ProjectionHost;

using System.Threading;
using System.Threading.Tasks;
using Autofac;
using BackOffice;
using BackOffice.Configuration;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using EventProcessors;
using Extensions;
using Hosts;
using Infrastructure.MartenDb.Setup;
using MartenMigration.Projections;
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
    public const int HostingPort = 10004;

    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = CreateHostBuilder(args).Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings([
                WellKnownConnectionNames.Events,
                WellKnownConnectionNames.EditorProjections,
                WellKnownConnectionNames.EditorProjectionsAdmin
            ])
            .Log((sp, logger) =>
            {
                var blobClientOptions = sp.GetRequiredService<BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
            })
            .RunAsync(async (sp, host, configuration) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var migratorFactories = sp.GetRequiredService<IRunnerDbContextMigratorFactory[]>();

                foreach (var migratorFactory in migratorFactories)
                {
                    await migratorFactory.CreateMigrator(configuration, loggerFactory)
                        .MigrateAsync(CancellationToken.None).ConfigureAwait(false);
                }
            });
    }

    public static RoadRegistryHostBuilder<Program> CreateHostBuilder(string[] args) => new RoadRegistryHostBuilder<Program>(args)
        .ConfigureServices((hostContext, services) =>
        {
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
                                hostContext.Configuration.GetRequiredConnectionString(WellKnownConnectionNames.EditorProjections),
                                options => options
                                    .EnableRetryOnFailure()
                                    .UseNetTopologySuite()
                            ).Options)
                )
                .AddSingleton<IRunnerDbContextMigratorFactory[]>([new EditorContextMigrationFactory(), new MartenMigrationContextMigrationFactory()])
                .AddEditorContextEventProcessor<RoadNetworkEventProcessor>(sp =>
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
                .AddEditorContextEventProcessor<OrganizationV2EventProcessor>(sp =>
                [
                    new OrganizationRecordV2Projection()
                ])
                .AddEditorContextEventProcessor<ChangeFeedEventProcessor>(sp =>
                [
                    new RoadNetworkChangeFeedProjection(sp.GetRequiredService<IBlobClient>())
                ])
                .AddEditorContextEventProcessor<ExtractDownloadEventProcessor>(sp =>
                [
                    new ExtractDownloadRecordProjection()
                ])
                .AddEditorContextEventProcessor<ExtractRequestEventProcessor>(sp =>
                [
                    new ExtractRequestRecordProjection(),
                    new ExtractRequestOverlapRecordProjection(sp.GetRequiredService<ILogger<ExtractRequestOverlapRecordProjection>>())
                ])
                .AddEditorContextEventProcessor<ExtractUploadEventProcessor>(sp =>
                [
                    new ExtractUploadRecordProjection()
                ])
                .AddEditorContextEventProcessor<RoadSegmentVersionEventProcessor>(sp =>
                [
                    new RoadSegmentVersionRecordProjection(sp.GetRequiredService<ILogger<RoadSegmentVersionRecordProjection>>())
                ])
                .AddMartenDbMigrationEventProcessor()
                ;
        })
        .ConfigureHealthChecks(HostingPort, builder => builder
            .AddHostedServicesStatus()
        )
        .ConfigureContainer((context, builder) =>
        {
            builder
                .Register(c => c.Resolve<RoadNetworkUploadsBlobClient>())
                .As<IBlobClient>().SingleInstance();
        });
}
