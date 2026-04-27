namespace RoadRegistry.BackOffice.CommandHost;

using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.FeatureCompare;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Handlers.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Hosts;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Infrastructure;
using RoadRegistry.Jobs;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.Snapshot.Handlers.Sqs;
using SqlStreamStore;
using MediatorModule = RoadRegistry.Snapshot.Handlers.Sqs.MediatorModule;

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
                CommandModules.RoadNetwork(sp),
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
