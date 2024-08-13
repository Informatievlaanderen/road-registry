namespace RoadRegistry.BackOffice.CommandHost;

using Abstractions;
using Autofac;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;
using Core;
using Extensions;
using Extracts;
using FeatureToggles;
using Framework;
using Handlers.Sqs;
using Hosts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using RoadNetwork.Schema;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using Snapshot.Handlers.Sqs;
using SqlStreamStore;
using System.Threading;
using System.Threading.Tasks;
using FeatureCompare.Readers;
using Jobs;
using Uploads;

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
                .AddRoadNetworkCommandQueue()
                .AddRoadNetworkEventWriter()
                .AddJobsContext()

                .AddHostedService<RoadNetworkCommandProcessor>()
                .AddHostedService<RoadNetworkExtractCommandProcessor>()

                .AddSingleton(new IDbContextMigratorFactory[]
                {
                    new RoadNetworkDbContextMigrationFactory(),
                    new JobsContextMigratorFactory()
                })
            )
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
            .ConfigureCommandDispatcher(sp => Resolve.WhenEqualToMessage([
                new RoadNetworkChangesArchiveCommandModule(
                    sp.GetRequiredService<RoadNetworkUploadsBlobClient>(),
                    sp.GetRequiredService<IStreamStore>(),
                    sp.GetRequiredService<ILifetimeScope>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                    sp.GetRequiredService<IZipArchiveBeforeFeatureCompareValidator>(),
                    sp.GetRequiredService<ITransactionZoneFeatureCompareFeatureReader>(),
                    sp.GetRequiredService<IClock>(),
                    sp.GetRequiredService<ILoggerFactory>()
                ),
                new RoadNetworkCommandModule(
                    sp.GetRequiredService<IStreamStore>(),
                    sp.GetRequiredService<ILifetimeScope>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                    sp.GetRequiredService<IClock>(),
                    sp.GetRequiredService<UseOvoCodeInChangeRoadNetworkFeatureToggle>(),
                    sp.GetService<IExtractUploadFailedEmailClient>(),
                    sp.GetService<IRoadNetworkEventWriter>(),
                    sp.GetRequiredService<ILoggerFactory>()
                ),
                new RoadNetworkExtractCommandModule(
                    sp.GetRequiredService<RoadNetworkExtractUploadsBlobClient>(),
                    sp.GetRequiredService<IStreamStore>(),
                    sp.GetRequiredService<ILifetimeScope>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                    sp.GetRequiredService<IZipArchiveBeforeFeatureCompareValidator>(),
                    sp.GetService<IExtractUploadFailedEmailClient>(),
                    sp.GetRequiredService<IClock>(),
                    sp.GetRequiredService<ILoggerFactory>()
                )
            ]))
            .ConfigureContainer((context, builder) =>
            {
                builder
                    .RegisterModule<Snapshot.Handlers.Sqs.MediatorModule>()
                    .RegisterModule<SqsHandlersModule>()
                    .RegisterModule<SnapshotSqsHandlersModule>();
            })
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new [] {
                WellKnownConnectionNames.Events,
                WellKnownConnectionNames.CommandHost,
                WellKnownConnectionNames.CommandHostAdmin,
                WellKnownConnectionNames.Jobs,
                WellKnownConnectionNames.JobsAdmin,
                WellKnownConnectionNames.Snapshots
            })
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
