namespace RoadRegistry.BackOffice.EventHost;

using Autofac;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
using Configuration;
using Core;
using Extensions;
using FeatureCompare.Readers;
using Framework;
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
                                sp.GetService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.EventHost)
                            ),
                            WellKnownSchemas.EventHostSchema))
                    .AddEditorContext()
                    .AddOrganizationCache()
                    .AddStreetNameCache()
                    .AddFeatureCompare()
                    .AddSingleton(sp => new EventHandlerModule[]
                    {
                        new RoadNetworkChangesArchiveEventModule(
                            sp.GetService<ILifetimeScope>(),
                            sp.GetRequiredService<RoadNetworkUploadsBlobClient>(),
                            sp.GetRequiredService<IStreamStore>(),
                            ApplicationMetadata,
                            sp.GetRequiredService<ITransactionZoneFeatureCompareFeatureReader>(),
                            sp.GetRequiredService<IRoadNetworkEventWriter>(),
                            sp.GetService<IExtractUploadFailedEmailClient>(),
                            sp.GetRequiredService<ILogger<RoadNetworkChangesArchiveEventModule>>()
                        ),
                        new RoadNetworkSnapshotEventModule(
                            sp.GetRequiredService<IStreamStore>(),
                            sp.GetRequiredService<ILifetimeScope>(),
                            sp.GetRequiredService<IMediator>(),
                            sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                            sp.GetRequiredService<IRoadNetworkSnapshotWriter>(),
                            sp.GetRequiredService<IClock>(),
                            sp.GetRequiredService<ILoggerFactory>())
                    })
                    .AddSingleton(sp => AcceptStreamMessage.WhenEqualToMessageType(sp.GetRequiredService<EventHandlerModule[]>(), EventProcessor.EventMapping))
                    .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(sp.GetRequiredService<EventHandlerModule[]>())));
            })
            .ConfigureHealthChecks(HostingPort, builder => builder
                .AddHostedServicesStatus()
            )
            .ConfigureContainer((context, builder) =>
            {
                builder
                    .RegisterModule<ContextModule>()
                    .RegisterModule<Snapshot.Handlers.Sqs.MediatorModule>()
                    .RegisterModule<SqsHandlersModule>()
                    .RegisterModule<SnapshotSqsHandlersModule>();

                builder
                    .Register(c => c.Resolve<RoadNetworkUploadsBlobClient>())
                    .As<IBlobClient>().SingleInstance();
            })
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new[] {
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
                        new SqlConnectionStringBuilder(configuration.GetRequiredConnectionString(WellKnownConnectionNames.SnapshotsAdmin))
                    ).CreateSchemaIfNotExists(WellKnownSchemas.SnapshotSchema).ConfigureAwait(false);
                await
                    new SqlEventProcessorPositionStoreSchema(
                        new SqlConnectionStringBuilder(configuration.GetRequiredConnectionString(WellKnownConnectionNames.EventHostAdmin))
                    ).CreateSchemaIfNotExists(WellKnownSchemas.EventHostSchema).ConfigureAwait(false);
            });
    }
}
