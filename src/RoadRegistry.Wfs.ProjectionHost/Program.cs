namespace RoadRegistry.Wfs.ProjectionHost;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Abstractions;
using BackOffice.FeatureToggles;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Hosts.Metadata;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Projections;
using Schema;
using StreetName;

public class Program
{
    public const int HostingPort = 10009;

    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) => services
                .AddSingleton(provider => provider.GetRequiredService<IConfiguration>().GetSection(MetadataConfiguration.Section).Get<MetadataConfiguration>())
                .AddStreetNameCache()
                .AddScoped<IMetadataUpdater, MetadataUpdater>()
                .AddSingleton(new EnvelopeFactory(
                    WfsContextEventProcessor.EventMapping,
                    new EventDeserializer((eventData, eventType) =>
                        JsonConvert.DeserializeObject(eventData, eventType, WfsContextEventProcessor.SerializerSettings)))
                )
                .AddDbContextFactory<WfsContext>((sp, options) =>
                {
                    var connectionString = sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.WfsProjections);
                    options
                        .UseSqlServer(connectionString,
                            o => o
                                .EnableRetryOnFailure()
                                .UseNetTopologySuite()
                        );
                })
                .AddSingleton(sp => new ConnectedProjection<WfsContext>[]
                {
                    // new GradeSeparatedJunctionRecordProjection(),
                    new RoadNodeRecordProjection(),
                    new RoadSegmentRecordProjection(
                        sp.GetRequiredService<IStreetNameCache>(),
                        sp.GetRequiredService<UseRoadSegmentSoftDeleteFeatureToggle>(),
                        sp.GetRequiredService<IStreetNameClient>())
                })
                .AddSingleton(sp => Resolve.WhenEqualToHandlerMessageType(sp
                    .GetRequiredService<ConnectedProjection<WfsContext>[]>()
                    .SelectMany(projection => projection.Handlers)
                    .ToArray())
                )
                .AddSingleton(sp =>
                    new AcceptStreamMessage<WfsContext>(
                        sp.GetRequiredService<ConnectedProjection<WfsContext>[]>()
                        , WfsContextEventProcessor.EventMapping))
                .AddSingleton<IRunnerDbContextMigratorFactory>(new WfsContextMigrationFactory())
                .AddStreetNameClient()
                .AddHostedService<WfsContextEventProcessor>())
            .ConfigureHealthChecks(HostingPort, builder => builder
                .AddHostedServicesStatus()
            )
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new[]
            {
                WellKnownConnectionNames.Events,
                WellKnownConnectionNames.WfsProjections,
                WellKnownConnectionNames.WfsProjectionsAdmin
            })
            .RunAsync(async (sp, host, configuration) =>
            {
                var migratorFactory = sp.GetRequiredService<IRunnerDbContextMigratorFactory>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                await migratorFactory.CreateMigrator(configuration, loggerFactory)
                    .MigrateAsync(CancellationToken.None).ConfigureAwait(false);
            });
    }
}
