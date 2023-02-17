namespace RoadRegistry.BackOffice.EventHost;

using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
using Core;
using FeatureToggles;
using Framework;
using Handlers;
using Handlers.Uploads;
using Hosts;
using Hosts.Configuration;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using Snapshot.Handlers;
using SqlStreamStore;
using System.Text;
using System.Threading.Tasks;
using Uploads;

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
                    .AddSingleton<IEventProcessorPositionStore>(sp =>
                        new SqlEventProcessorPositionStore(
                            new SqlConnectionStringBuilder(
                                sp.GetService<IConfiguration>().GetConnectionString(WellknownConnectionNames.EventHost)
                            ),
                            WellknownSchemas.EventHostSchema))
                    .AddSingleton(sp => new EventHandlerModule[]
                    {
                        new RoadNetworkChangesArchiveEventModule(
                            sp.GetService<RoadNetworkUploadsBlobClient>(),
                            new ZipArchiveTranslator(Encoding.GetEncoding(1252)),
                            sp.GetService<IStreamStore>(),
                            ApplicationMetadata,
                            sp.GetService<ILogger<RoadNetworkChangesArchiveEventModule>>()
                        ),
                        new RoadNetworkBackOfficeEventModule(
                            sp.GetService<IStreamStore>(),
                            sp.GetService<IRoadNetworkSnapshotReader>(),
                            sp.GetService<IRoadNetworkSnapshotWriter>(),
                            sp.GetService<IClock>(),
                            sp.GetService<ILoggerFactory>()),
                        new RoadNetworkSnapshotEventModule(
                            sp.GetService<IStreamStore>(),
                            sp.GetService<IMediator>(),
                            sp.GetService<IRoadNetworkSnapshotReader>(),
                            sp.GetService<IRoadNetworkSnapshotWriter>(),
                            sp.GetService<IClock>(),
                            sp.GetService<ILoggerFactory>(),
                            sp.GetService<UseSnapshotSqsRequestFeatureToggle>())
                    })
                    .AddSingleton(sp => AcceptStreamMessage.WhenEqualToMessageType(sp.GetRequiredService<EventHandlerModule[]>(), EventProcessor.EventMapping))
                    .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(sp.GetRequiredService<EventHandlerModule[]>())));
            })
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new []
            {
                WellknownConnectionNames.Events,
                WellknownConnectionNames.EventHost,
                WellknownConnectionNames.EventHostAdmin,
                WellknownConnectionNames.Snapshots,
                WellknownConnectionNames.SnapshotsAdmin
            })
            .Log((sp, logger) =>
            {
                var blobClientOptions = sp.GetService<BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
            })
            .RunAsync(async (sp, host, configuration) =>
            {
                var blobClient = sp.GetService<IBlobClient>();
                await
                    new SqlBlobSchema(
                        new SqlConnectionStringBuilder(configuration.GetConnectionString(WellknownConnectionNames.SnapshotsAdmin))
                    ).CreateSchemaIfNotExists(WellknownSchemas.SnapshotSchema).ConfigureAwait(false);
                await
                    new SqlEventProcessorPositionStoreSchema(
                        new SqlConnectionStringBuilder(configuration.GetConnectionString(WellknownConnectionNames.EventHostAdmin))
                    ).CreateSchemaIfNotExists(WellknownSchemas.EventHostSchema).ConfigureAwait(false);
                await blobClient.ProvisionResources(host).ConfigureAwait(false);
            });
    }
}
