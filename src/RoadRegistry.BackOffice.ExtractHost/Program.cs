namespace RoadRegistry.BackOffice.ExtractHost;

using Abstractions;
using Autofac;
using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
using Configuration;
using Editor.Schema;
using Extensions;
using Extracts;
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
using SqlStreamStore;
using System;
using System.Threading.Tasks;
using ZipArchiveWriters.ExtractHost;

public class Program
{
    public const int HostingPort = 10003;

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
                    .AddEmailClient()
                    .AddRoadRegistrySnapshot()
                    .AddRoadNetworkEventWriter()
                    .AddScoped(_ => new EventSourcedEntityMap())
                    .AddSingleton<IEventProcessorPositionStore>(sp =>
                        new SqlEventProcessorPositionStore(
                            new SqlConnectionStringBuilder(
                                sp.GetService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.ExtractHost)
                            ),
                            WellKnownSchemas.ExtractHostSchema))
                    .AddSingleton<IZipArchiveWriter<EditorContext>>(sp =>
                        new RoadNetworkExtractToZipArchiveWriter(
                            sp.GetService<ZipArchiveWriterOptions>(),
                            sp.GetService<IStreetNameCache>(),
                            sp.GetService<RecyclableMemoryStreamManager>(),
                            sp.GetRequiredService<FileEncoding>(),
                            sp.GetRequiredService<ILogger<RoadNetworkExtractToZipArchiveWriter>>()
                        ))
                    .AddSingleton<IRoadNetworkExtractArchiveAssembler>(sp =>
                        new RoadNetworkExtractArchiveAssembler(
                            sp.GetService<RecyclableMemoryStreamManager>(),
                            sp.GetService<Func<EditorContext>>(),
                            sp.GetService<IZipArchiveWriter<EditorContext>>()))
                    .AddEditorContext()
                    .AddOrganizationCache()
                    .AddStreetNameCache()
                    .AddFeatureCompare()
                    .AddSingleton(sp => new EventHandlerModule[]
                    {
                        new RoadNetworkExtractEventModule(
                            sp.GetService<ILifetimeScope>(),
                            sp.GetService<RoadNetworkExtractDownloadsBlobClient>(),
                            sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                            sp.GetService<IRoadNetworkExtractArchiveAssembler>(),
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
                    .CheckPermission(WellKnownBuckets.SnapshotsBucket, Permission.Read)
                    .CheckPermission(WellKnownBuckets.SqsMessagesBucket, Permission.Read)
                    .CheckPermission(WellKnownBuckets.UploadsBucket, Permission.Read)
                    .CheckPermission(WellKnownBuckets.ExtractDownloadsBucket, Permission.Read, Permission.Delete)
                )
                .AddSqs(x => x
                    .CheckPermission(WellKnownQueues.SnapshotQueue, Permission.Read)
                )
            )
            .ConfigureContainer((context, builder) =>
            {
                builder
                    .RegisterModule<ContextModule>();
            })
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new []
            {
                WellKnownConnectionNames.Events,
                WellKnownConnectionNames.ExtractHost,
                WellKnownConnectionNames.ExtractHostAdmin,
                WellKnownConnectionNames.Snapshots,
                WellKnownConnectionNames.SnapshotsAdmin,
                WellKnownConnectionNames.EditorProjections,
                WellKnownConnectionNames.SyndicationProjections
            })
            .Log((sp, logger) => {
                var blobClientOptions = sp.GetService<BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
            })
            .RunAsync(async (sp, host, configuration) =>
            {
                await new SqlBlobSchema(new SqlConnectionStringBuilder(configuration.GetRequiredConnectionString(WellKnownConnectionNames.SnapshotsAdmin))).CreateSchemaIfNotExists(WellKnownSchemas.SnapshotSchema).ConfigureAwait(false);
                await new SqlEventProcessorPositionStoreSchema(new SqlConnectionStringBuilder(configuration.GetRequiredConnectionString(WellKnownConnectionNames.ExtractHostAdmin))).CreateSchemaIfNotExists(WellKnownSchemas.ExtractHostSchema).ConfigureAwait(false);
            });
    }
}
