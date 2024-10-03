namespace RoadRegistry.SyncHost.Infrastructure;

using Autofac;
using BackOffice;
using BackOffice.Extensions;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.Projector.Modules;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules;
using Sync.OrganizationRegistry;
using Sync.StreetNameRegistry;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    public const int HostingPort = 10008;

    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) =>
            {
                services
                    .AddSingleton(new ApplicationMetadata(RoadRegistryApplication.BackOffice))
                    .AddHttpClient()
                    .AddScoped(_ => new EventSourcedEntityMap())
                    .AddRoadNetworkCommandQueue()
                    .AddRoadNetworkEventWriter()
                    .AddEditorContext()
                    .RegisterOptions<KafkaOptions>()

                    .AddOrganizationConsumerServices()
                    .AddHostedService<OrganizationConsumer>()

                    .AddStreetNameConsumerServices()
                    .AddHostedService<StreetNameEventConsumer>()
                    .AddHostedService<StreetNameSnapshotConsumer>()

                    .AddStreetNameProjectionServices()
                    .AddHostedService<StreetNameEventProjectionContextEventProcessor>()
                    .AddHostedService<StreetNameSnapshotProjectionContextEventProcessor>()

                    .AddSingleton(new IDbContextMigratorFactory[]
                    {
                        new OrganizationConsumerContextMigratorFactory(),
                        new StreetNameEventConsumerContextMigrationFactory(),
                        new StreetNameEventProjectionContextMigrationFactory(),
                        new StreetNameSnapshotConsumerContextMigrationFactory(),
                        new StreetNameSnapshotProjectionContextMigrationFactory()
                    })
                    ;
            })
            .ConfigureHealthChecks(HostingPort,builder => builder
                .AddHostedServicesStatus()
            )
            .ConfigureContainer((hostContext, builder) =>
                {
                    builder
                        .RegisterModule(new ApiModule(hostContext.Configuration))
                        .RegisterModule(new ProjectorModule(hostContext.Configuration));
                }
            )
            .Build();

        await roadRegistryHost
            .RunAsync(async (sp, host, configuration) =>
            {
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
