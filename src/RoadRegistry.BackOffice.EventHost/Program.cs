namespace RoadRegistry.BackOffice.EventHost;

using System.Threading.Tasks;
using Autofac;
using Be.Vlaanderen.Basisregisters.BlobStore;
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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using Snapshot.Handlers;
using Snapshot.Handlers.Sqs;
using SqlStreamStore;
using Uploads;
using MediatorModule = Snapshot.Handlers.Sqs.MediatorModule;

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
                    .AddSingleton(sp => AcceptStreamMessage.WhenEqualToMessageType(sp.GetRequiredService<EventHandlerModule[]>(), PositionStoreEventProcessor.EventMapping))
                    .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(sp.GetRequiredService<EventHandlerModule[]>())))

                    .AddHostedService<EventProcessor>()
                    .AddHostedService(sp =>
                    {
                        var modules = new EventHandlerModule[]
                        {
                            new EventHostHealthModule(
                                sp.GetRequiredService<ILifetimeScope>(),
                                sp.GetRequiredService<RoadNetworkUploadsBlobClient>(),
                                sp.GetRequiredService<ILoggerFactory>()),
                        };

                        var acceptStreamMessageFilter = AcceptStreamMessage.WhenEqualToMessageType(modules, PositionStoreEventProcessor.EventMapping);
                        var eventHandlerDispatcher = Dispatch.Using(Resolve.WhenEqualToMessage(modules));

                        return new HealthEventProcessor(
                            sp.GetRequiredService<IHostApplicationLifetime>(),
                            sp.GetRequiredService<IStreamStore>(),
                            sp.GetRequiredService<IEventProcessorPositionStore>(),
                            acceptStreamMessageFilter,
                            eventHandlerDispatcher,
                            sp.GetRequiredService<Scheduler>(),
                            sp.GetRequiredService<ILoggerFactory>()
                        );
                    });
            })
            .ConfigureHealthChecks(HostingPort, builder => builder
                .AddHostedServicesStatus()
            )
            .ConfigureContainer((context, builder) =>
            {
                builder
                    .RegisterModule<ContextModule>()
                    .RegisterModule<MediatorModule>()
                    .RegisterModule<SqsHandlersModule>()
                    .RegisterModule<SnapshotSqsHandlersModule>();

                builder
                    .Register(c => c.Resolve<RoadNetworkUploadsBlobClient>())
                    .As<IBlobClient>()
                    .SingleInstance();
            })
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings([
                WellKnownConnectionNames.Events,
                WellKnownConnectionNames.EventHost,
                WellKnownConnectionNames.EventHostAdmin
            ])
            .Log((sp, logger) =>
            {
                var blobClientOptions = sp.GetService<BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
            })
            .RunAsync(async (sp, host, configuration) =>
            {
                await
                    new SqlEventProcessorPositionStoreSchema(
                        new SqlConnectionStringBuilder(configuration.GetRequiredConnectionString(WellKnownConnectionNames.EventHostAdmin))
                    ).CreateSchemaIfNotExists(WellKnownSchemas.EventHostSchema).ConfigureAwait(false);
            });
    }
}
