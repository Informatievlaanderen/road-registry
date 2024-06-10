namespace RoadRegistry.Integration.ProjectionHost;

using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using EventProcessors;
using Extensions;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Projections;
using Schema;

public class Program
{
    private const int HostingPort = 10011;

    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = CreateHostBuilder(args).Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings([
                WellKnownConnectionNames.Events,
                WellKnownConnectionNames.IntegrationProjections,
                WellKnownConnectionNames.IntegrationProjectionsAdmin
            ])
            .RunAsync(async (sp, _, configuration) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var migratorFactory = sp.GetRequiredService<IRunnerDbContextMigratorFactory>();

                await migratorFactory.CreateMigrator(configuration, loggerFactory)
                    .MigrateAsync(CancellationToken.None).ConfigureAwait(false);
            });
    }

    public static RoadRegistryHostBuilder<Program> CreateHostBuilder(string[] args) => new RoadRegistryHostBuilder<Program>(args)
        .ConfigureServices((hostContext, services) =>
        {
            services
                .AddSingleton(new EnvelopeFactory(
                    IntegrationContextEventProcessor.EventMapping,
                    new EventDeserializer((eventData, eventType) =>
                        JsonConvert.DeserializeObject(eventData, eventType, IntegrationContextEventProcessor.SerializerSettings)))
                )
                .AddSingleton(() =>
                    new IntegrationContext(
                        new DbContextOptionsBuilder<IntegrationContext>()
                            .UseNpgsql(
                                hostContext.Configuration.GetRequiredConnectionString(WellKnownConnectionNames.IntegrationProjections),
                                options => options
                                    .EnableRetryOnFailure()
                                    .MigrationsHistoryTable(MigrationTables.Integration, WellKnownSchemas.IntegrationSchema)
                                    .UseNetTopologySuite()
                            ).Options)
                )
                .AddSingleton<IRunnerDbContextMigratorFactory>(new IntegrationContextMigrationFactory())
                .AddIntegrationContextEventProcessor<RoadNetworkEventProcessor>(_ =>
                [
                    new RoadSegmentLatestItemProjection()
                ])
                // .AddIntegrationContextEventProcessor<OrganizationEventProcessor>(sp => new ConnectedProjection<IntegrationContext>[]
                // {
                //     new OrganizationRecordProjection(
                //         sp.GetRequiredService<RecyclableMemoryStreamManager>(),
                //         sp.GetRequiredService<FileEncoding>(),
                //         sp.GetRequiredService<ILogger<OrganizationRecordProjection>>())
                // })
                ;
        })
        .ConfigureHealthChecks(HostingPort, _ => { }
        );
}
