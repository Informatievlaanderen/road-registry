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
using RoadRegistry.StreetNameConsumer.Schema;
using Sync.OrganizationRegistry;
using System.Threading;
using System.Threading.Tasks;
using Sync.StreetNameRegistry;

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
                    .AddSingleton<IStreetNameEventWriter, StreetNameEventWriter>()
                    .AddEditorContext()
                    .RegisterOptions<OrganizationConsumerOptions>()
                    .RegisterOptions<KafkaOptions>()
                    .AddSingleton<IStreetNameTopicConsumer, StreetNameTopicConsumer>()
                    .AddSingleton(new IDbContextMigratorFactory[]
                    {
                        new OrganizationConsumerContextMigratorFactory(),
                        new StreetNameConsumerContextMigrationFactory()
                    })
                    .AddHostedService<OrganizationConsumer>()
                    .AddHostedService<StreetNameConsumer>()
                    //TODO-rik add projector for StreetNameConsumerProjection
                    ;
            })
            .ConfigureHealthChecks(HostingPort,builder => builder
                .AddSqlServer()
                .AddHostedServicesStatus()
                .AddKafka()
            )
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
                var migratorFactories = sp.GetRequiredService<IDbContextMigratorFactory[]>();

                foreach (var migratorFactory in migratorFactories)
                {
                    await migratorFactory.CreateMigrator(configuration, loggerFactory)
                        .MigrateAsync(CancellationToken.None).ConfigureAwait(false);
                }
            });
    }
}
