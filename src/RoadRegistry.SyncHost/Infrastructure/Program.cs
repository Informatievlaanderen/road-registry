namespace RoadRegistry.SyncHost.Infrastructure;

using Autofac;
using BackOffice.Extensions;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.Projector.Modules;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules;
using RoadRegistry.StreetNameConsumer.Schema;
using Sync.OrganizationRegistry;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) =>
            {
                services
                    .AddSingleton(new IRunnerDbContextMigratorFactory[]
                    {
                        new OrganizationConsumerContextMigrationFactory(),
                        new StreetNameConsumerContextMigrationFactory()
                    })
                    .AddSingleton(new ApplicationMetadata(RoadRegistryApplication.BackOffice))
                    .AddHttpClient()
                    .AddScoped(_ => new EventSourcedEntityMap())
                    .AddRoadNetworkCommandQueue()
                    .AddEditorContext()
                    .RegisterOptions<OrganizationConsumerOptions>()
                    .RegisterOptions<KafkaOptions>()
                    .AddHostedService<OrganizationConsumer>()
                    .AddHostedService<StreetNameConsumer>()
                    ;
            })
            .ConfigureContainer((hostContext, builder) =>
                {
                    builder
                        .RegisterModule<OrganizationConsumerModule>()
                        .RegisterModule<StreetNameConsumerModule>()
                        .RegisterModule(new ApiModule(hostContext.Configuration))
                        .RegisterModule(new ProjectorModule(hostContext.Configuration));
                }
            )
            .Build();
        
        await roadRegistryHost
            .RunAsync(async (sp, host, configuration) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var migratorFactories = sp.GetRequiredService<IRunnerDbContextMigratorFactory[]>();

                foreach (var migratorFactory in migratorFactories)
                {
                    await migratorFactory.CreateMigrator(configuration, loggerFactory)
                        .MigrateAsync(CancellationToken.None).ConfigureAwait(false);
                }
            });
    }
}
