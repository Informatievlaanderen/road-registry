namespace RoadRegistry.BackOffice.CommandHost;

using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Autofac;
using Core;
using Extensions;
using Extracts;
using FeatureCompare;
using Framework;
using Handlers.Extracts;
using Handlers.Sqs;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Jobs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using RoadNetwork.Schema;
using RoadRegistry.Extracts.Schema;
using Snapshot.Handlers.Sqs;
using SqlStreamStore;
using Uploads;
using MediatorModule = Snapshot.Handlers.Sqs.MediatorModule;

public class Program
{
    public const int HostingPort = 10000;

    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) => services
                .AddTicketing()
                .AddEmailClient()
                .AddRoadRegistrySnapshot()
                .AddScoped(_ => new EventSourcedEntityMap())
                .AddSingleton<ICommandProcessorPositionStore>(sp =>
                    new SqlCommandProcessorPositionStore(
                        new SqlConnectionStringBuilder(
                            sp.GetService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.CommandHost)
                        ),
                        WellKnownSchemas.CommandHostSchema))
                .AddDistributedStreamStoreLockOptions()
                .AddRoadNetworkDbIdGenerator()
                .AddEditorContext()
                .AddOrganizationCache()
                .AddStreetNameCache()
                .AddFeatureCompare()
                .AddHealthCommandQueue()
                .AddRoadNetworkCommandQueue()
                .AddRoadNetworkEventWriter()
                .AddOrganizationCommandQueue()
                .AddJobsContext()
                .AddExtractsDbContext(QueryTrackingBehavior.TrackAll)
                .AddScoped<IExtractRequests, ExtractRequests>()

                .AddHostedService<HealthCommandProcessor>()
                .AddHostedService<RoadNetworkCommandProcessor>()
                .AddHostedService<RoadNetworkExtractCommandProcessor>()
                .AddHostedService<OrganizationCommandProcessor>()

                .AddSingleton(new IDbContextMigratorFactory[]
                {
                    new RoadNetworkDbContextMigrationFactory(),
                    new JobsContextMigratorFactory()
                })
            )
            .ConfigureHealthChecks(HostingPort, builder => builder
                .AddHostedServicesStatus()
            )
            .ConfigureCommandDispatcher(sp => Resolve.WhenEqualToMessage([
                new CommandHostHealthModule(
                    sp.GetRequiredService<IStreamStore>(),
                    sp.GetRequiredService<ILifetimeScope>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                    sp.GetRequiredService<IClock>(),
                    sp.GetRequiredService<ILoggerFactory>()
                ),
                new RoadNetworkChangesArchiveCommandModule(
                    sp.GetRequiredService<RoadNetworkUploadsBlobClient>(),
                    sp.GetRequiredService<IStreamStore>(),
                    sp.GetRequiredService<ILifetimeScope>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                    sp.GetRequiredService<IZipArchiveBeforeFeatureCompareValidatorFactory>(),
                    sp.GetRequiredService<ITransactionZoneZipArchiveReader>(),
                    sp.GetRequiredService<IClock>(),
                    sp.GetRequiredService<ILoggerFactory>()
                ),
                new RoadNetworkCommandModule(
                    sp.GetRequiredService<IStreamStore>(),
                    sp.GetRequiredService<ILifetimeScope>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                    sp.GetRequiredService<IClock>(),
                    sp.GetService<IExtractUploadFailedEmailClient>(),
                    sp.GetRequiredService<ILoggerFactory>()
                ),
                new RoadNetworkExtractCommandModule(
                    sp.GetRequiredService<RoadNetworkExtractUploadsBlobClient>(),
                    sp.GetRequiredService<IStreamStore>(),
                    sp.GetRequiredService<ILifetimeScope>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                    sp.GetRequiredService<IZipArchiveBeforeFeatureCompareValidatorFactory>(),
                    sp.GetService<IExtractUploadFailedEmailClient>(),
                    sp.GetRequiredService<IClock>(),
                    sp.GetRequiredService<ILoggerFactory>()
                ),
                new OrganizationCommandModule(
                    sp.GetRequiredService<IStreamStore>(),
                    sp.GetRequiredService<ILifetimeScope>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                    sp.GetRequiredService<IClock>(),
                    sp.GetRequiredService<ILoggerFactory>()
                )
            ]))
            .ConfigureContainer((context, builder) =>
            {
                builder
                    .RegisterModule<MediatorModule>()
                    .RegisterModule<SqsHandlersModule>()
                    .RegisterModule<SnapshotSqsHandlersModule>();
            })
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings([
                WellKnownConnectionNames.Events,
                WellKnownConnectionNames.CommandHost,
                WellKnownConnectionNames.CommandHostAdmin,
                WellKnownConnectionNames.Jobs,
                WellKnownConnectionNames.JobsAdmin
            ])
            .RunAsync(async (sp, host, configuration) =>
            {
                await new SqlCommandProcessorPositionStoreSchema(
                    new SqlConnectionStringBuilder(
                        configuration.GetRequiredConnectionString(WellKnownConnectionNames.CommandHostAdmin))
                    )
                    .CreateSchemaIfNotExists(WellKnownSchemas.CommandHostSchema).ConfigureAwait(false);

                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var migratorFactories = sp.GetRequiredService<IDbContextMigratorFactory[]>();

                foreach (var migratorFactory in migratorFactories)
                {
                    await migratorFactory.CreateMigrator(configuration, loggerFactory)
                        .MigrateAsync(CancellationToken.None).ConfigureAwait(false);
                }
            });
    }
}
