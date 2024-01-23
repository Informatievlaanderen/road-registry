namespace RoadRegistry.BackOffice.EventHost;

using Autofac;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
using Configuration;
using Core;
using Extensions;
using FeatureCompare;
using FeatureCompare.Translators;
using FeatureToggles;
using Framework;
using Handlers;
using Handlers.Sqs;
using Handlers.Uploads;
using Hosts;
using Hosts.Infrastructure.Extensions;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using Snapshot.Handlers;
using Snapshot.Handlers.Sqs;
using SqlStreamStore;
using System.Threading.Tasks;
using Uploads;

public class Program
{
    public const int HostingPort = 10001;

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
                    .AddEmailClient()
                    .AddTicketing()
                    .AddRoadRegistrySnapshot()
                    .AddRoadNetworkEventWriter()
                    .AddScoped(_ => new EventSourcedEntityMap())
                    .AddSingleton<IEventProcessorPositionStore>(sp =>
                        new SqlEventProcessorPositionStore(
                            new SqlConnectionStringBuilder(
                                sp.GetService<IConfiguration>().GetConnectionString(WellKnownConnectionNames.EventHost)
                            ),
                            WellKnownSchemas.EventHostSchema))
                    .AddEditorContext()
                    .AddOrganizationRepository()
                    .AddFeatureCompareTranslator()
                    .AddSingleton(sp => new EventHandlerModule[]
                    {
                        new RoadNetworkChangesArchiveEventModule(
                            sp.GetService<ILifetimeScope>(),
                            sp.GetRequiredService<RoadNetworkUploadsBlobClient>(),
                            new ZipArchiveTranslator(sp.GetRequiredService<FileEncoding>(), sp.GetRequiredService<ILogger<ZipArchiveTranslator>>()),
                            sp.GetRequiredService<IStreamStore>(),
                            ApplicationMetadata,
                            sp.GetRequiredService<TransactionZoneFeatureCompareFeatureReader>(),
                            sp.GetRequiredService<IRoadNetworkEventWriter>(),
                            sp.GetService<IExtractUploadFailedEmailClient>(),
                            sp.GetRequiredService<ILogger<RoadNetworkChangesArchiveEventModule>>()
                        ),
                        new RoadNetworkBackOfficeEventModule(
                            sp.GetRequiredService<IStreamStore>(),
                            sp.GetRequiredService<ILifetimeScope>(),
                            sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                            sp.GetRequiredService<IRoadNetworkSnapshotWriter>(),
                            sp.GetRequiredService<IClock>(),
                            sp.GetRequiredService<ILoggerFactory>()),
                        new RoadNetworkSnapshotEventModule(
                            sp.GetRequiredService<IStreamStore>(),
                            sp.GetRequiredService<ILifetimeScope>(),
                            sp.GetRequiredService<IMediator>(),
                            sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                            sp.GetRequiredService<IRoadNetworkSnapshotWriter>(),
                            sp.GetRequiredService<IClock>(),
                            sp.GetRequiredService<ILoggerFactory>(),
                            sp.GetRequiredService<UseSnapshotSqsRequestFeatureToggle>())
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
                )
                .AddSqs(x => x
                    .CheckPermission(WellKnownQueues.SnapshotQueue, Permission.Read)
                )
                .AddTicketing()
            )
            .ConfigureContainer((context, builder) =>
            {
                builder
                    .RegisterModule<ContextModule>()
                    .RegisterModule<RoadRegistry.Snapshot.Handlers.Sqs.MediatorModule>()
                    .RegisterModule<SqsHandlersModule>()
                    .RegisterModule<SnapshotSqsHandlersModule>();

                builder
                    .Register(c => c.Resolve<RoadNetworkUploadsBlobClient>())
                    .As<IBlobClient>().SingleInstance();
            })
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new []
            {
                WellKnownConnectionNames.Events,
                WellKnownConnectionNames.EventHost,
                WellKnownConnectionNames.EventHostAdmin,
                WellKnownConnectionNames.Snapshots,
                WellKnownConnectionNames.SnapshotsAdmin
            })
            .Log((sp, logger) =>
            {
                var blobClientOptions = sp.GetService<BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
            })
            .RunAsync(async (sp, host, configuration) =>
            {
                await
                    new SqlBlobSchema(
                        new SqlConnectionStringBuilder(configuration.GetConnectionString(WellKnownConnectionNames.SnapshotsAdmin))
                    ).CreateSchemaIfNotExists(WellKnownSchemas.SnapshotSchema).ConfigureAwait(false);
                await
                    new SqlEventProcessorPositionStoreSchema(
                        new SqlConnectionStringBuilder(configuration.GetConnectionString(WellKnownConnectionNames.EventHostAdmin))
                    ).CreateSchemaIfNotExists(WellKnownSchemas.EventHostSchema).ConfigureAwait(false);
            });
    }
}
