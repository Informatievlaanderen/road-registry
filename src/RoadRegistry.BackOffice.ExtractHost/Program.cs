namespace RoadRegistry.BackOffice.ExtractHost;

using Abstractions;
using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
using Configuration;
using Editor.Schema;
using Extensions;
using Extracts;
using FeatureCompare;
using Framework;
using Handlers.Extracts;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using RoadRegistry.BackOffice.FeatureToggles;
using SqlStreamStore;
using System;
using System.Threading.Tasks;
using Uploads;
using ZipArchiveWriters.ExtractHost;

public class Program
{
    public const int HostingPort = 10011;

    private static readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.BackOffice);

    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) =>
            {
                services
                    .AddHostedService<EventProcessor>()
                    .RegisterOptions<ZipArchiveWriterOptions>()
                    .AddEmailClient(hostContext.Configuration)
                    .AddRoadRegistrySnapshot()
                    .AddRoadNetworkEventWriter()
                    .AddSingleton<IEventProcessorPositionStore>(sp =>
                        new SqlEventProcessorPositionStore(
                            new SqlConnectionStringBuilder(
                                sp.GetService<IConfiguration>().GetConnectionString(WellknownConnectionNames.ExtractHost)
                            ),
                            WellknownSchemas.ExtractHostSchema))
                    .AddStreetNameCache()
                    .AddSingleton<IZipArchiveWriter<EditorContext>>(sp =>
                        new RoadNetworkExtractToZipArchiveWriter(
                            sp.GetService<ZipArchiveWriterOptions>(),
                            sp.GetService<IStreetNameCache>(),
                            sp.GetService<RecyclableMemoryStreamManager>(),
                            sp.GetRequiredService<FileEncoding>(),
                            sp.GetRequiredService<UseNetTopologySuiteShapeReaderWriterFeatureToggle>()
                        ))
                    .AddSingleton<Func<EditorContext>>(sp =>
                        () =>
                            new EditorContext(
                                new DbContextOptionsBuilder<EditorContext>()
                                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                                    .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                                    .UseSqlServer(
                                        hostContext.Configuration.GetConnectionString(WellknownConnectionNames.EditorProjections),
                                        options => options
                                            .UseNetTopologySuite()
                                    ).Options)
                    )
                    .AddSingleton<IRoadNetworkExtractArchiveAssembler>(sp =>
                        new RoadNetworkExtractArchiveAssembler(
                            sp.GetService<RecyclableMemoryStreamManager>(),
                            sp.GetService<Func<EditorContext>>(),
                            sp.GetService<IZipArchiveWriter<EditorContext>>()))
                    .AddSingleton(sp => new EventHandlerModule[]
                    {
                        new RoadNetworkExtractEventModule(
                            sp.GetService<RoadNetworkExtractDownloadsBlobClient>(),
                            sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                            sp.GetService<IRoadNetworkExtractArchiveAssembler>(),
                            new ZipArchiveTranslator(sp.GetRequiredService<FileEncoding>()),
                            new ZipArchiveFeatureCompareTranslator(
                                sp.GetRequiredService<FileEncoding>(),
                                sp.GetRequiredService<ILogger<ZipArchiveFeatureCompareTranslator>>(),
                                sp.GetRequiredService<UseGradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegmentValidationFeatureToggle>()
                            ),
                            sp.GetService<IStreamStore>(),
                            ApplicationMetadata,
                            sp.GetService<IRoadNetworkEventWriter>(),
                            sp.GetService<IExtractUploadFailedEmailClient>(),
                            sp.GetService<ILogger<RoadNetworkExtractEventModule>>())
                    })
                    .AddSingleton(sp => AcceptStreamMessage.WhenEqualToMessageType(sp.GetRequiredService<EventHandlerModule[]>(), EventProcessor.EventMapping))
                    .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(sp.GetRequiredService<EventHandlerModule[]>())));
            })
            .ConfigureHealthChecks(HostingPort, builder => builder
                .AddSqlServer()
                .AddHostedServicesStatus()
                .AddS3(x => x
                    .CheckPermission(WellknownBuckets.SnapshotsBucket, Permission.Read)
                    .CheckPermission(WellknownBuckets.SqsMessagesBucket, Permission.Read)
                    .CheckPermission(WellknownBuckets.UploadsBucket, Permission.Read)
                    .CheckPermission(WellknownBuckets.ExtractDownloadsBucket, Permission.Read, Permission.Delete)
                )
                .AddSqs(x => x
                    .CheckPermission(WellknownQueues.SnapshotQueue, Permission.Read)
                )
            )
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new []
            {
                WellknownConnectionNames.Events,
                WellknownConnectionNames.ExtractHost,
                WellknownConnectionNames.ExtractHostAdmin,
                WellknownConnectionNames.Snapshots,
                WellknownConnectionNames.SnapshotsAdmin,
                WellknownConnectionNames.EditorProjections,
                WellknownConnectionNames.SyndicationProjections
            })
            .Log((sp, logger) => {
                var blobClientOptions = sp.GetService<BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
            })
            .RunAsync(async (sp, host, configuration) =>
            {
                await new SqlBlobSchema(new SqlConnectionStringBuilder(configuration.GetConnectionString(WellknownConnectionNames.SnapshotsAdmin))).CreateSchemaIfNotExists(WellknownSchemas.SnapshotSchema).ConfigureAwait(false);
                await new SqlEventProcessorPositionStoreSchema(new SqlConnectionStringBuilder(configuration.GetConnectionString(WellknownConnectionNames.ExtractHostAdmin))).CreateSchemaIfNotExists(WellknownSchemas.ExtractHostSchema).ConfigureAwait(false);
            });
    }
}
