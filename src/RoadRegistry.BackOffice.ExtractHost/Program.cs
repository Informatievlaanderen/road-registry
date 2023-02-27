namespace RoadRegistry.BackOffice.ExtractHost;

using Abstractions;
using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
using Configuration;
using Editor.Schema;
using Extensions;
using Extracts;
using Framework;
using Handlers.Extracts;
using Hosts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using SqlStreamStore;
using Syndication.Schema;
using System;
using System.Text;
using System.Threading.Tasks;
using Uploads;
using ZipArchiveWriters.ExtractHost;

public class Program
{
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
                    .AddSingleton<IEventProcessorPositionStore>(sp =>
                        new SqlEventProcessorPositionStore(
                            new SqlConnectionStringBuilder(
                                sp.GetService<IConfiguration>().GetConnectionString(WellknownConnectionNames.ExtractHost)
                            ),
                            WellknownSchemas.ExtractHostSchema))
                    .AddSingleton<IStreetNameCache, StreetNameCache>()
                    .AddSingleton<Func<SyndicationContext>>(sp =>
                        () =>
                            new SyndicationContext(
                                new DbContextOptionsBuilder<SyndicationContext>()
                                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                                    .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                                    .UseSqlServer(
                                        hostContext.Configuration.GetConnectionString(WellknownConnectionNames.SyndicationProjections),
                                        options => options
                                            .EnableRetryOnFailure()
                                    ).Options)
                    )
                    .AddSingleton<IZipArchiveWriter<EditorContext>>(sp =>
                        new RoadNetworkExtractToZipArchiveWriter(
                            sp.GetService<ZipArchiveWriterOptions>(),
                            sp.GetService<IStreetNameCache>(),
                            sp.GetService<RecyclableMemoryStreamManager>(),
                            Encoding.GetEncoding(1252)))
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
                            new ZipArchiveTranslator(Encoding.GetEncoding(1252)),
                            sp.GetService<IStreamStore>(),
                            ApplicationMetadata)
                    })
                    .AddSingleton(sp => AcceptStreamMessage.WhenEqualToMessageType(sp.GetRequiredService<EventHandlerModule[]>(), EventProcessor.EventMapping))
                    .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(sp.GetRequiredService<EventHandlerModule[]>())));
            })
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
                var blobClient = sp.GetService<RoadNetworkExtractDownloadsBlobClient>();

                await new SqlBlobSchema(new SqlConnectionStringBuilder(configuration.GetConnectionString(WellknownConnectionNames.SnapshotsAdmin))).CreateSchemaIfNotExists(WellknownSchemas.SnapshotSchema).ConfigureAwait(false);
                await new SqlEventProcessorPositionStoreSchema(new SqlConnectionStringBuilder(configuration.GetConnectionString(WellknownConnectionNames.ExtractHostAdmin))).CreateSchemaIfNotExists(WellknownSchemas.ExtractHostSchema).ConfigureAwait(false);
                await blobClient.ProvisionResources(host).ConfigureAwait(false);
            });
    }
}
