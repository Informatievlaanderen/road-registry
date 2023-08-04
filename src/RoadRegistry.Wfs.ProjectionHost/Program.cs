namespace RoadRegistry.Wfs.ProjectionHost;

using BackOffice;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Hosts.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Projections;
using Schema;
using Syndication.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.FeatureToggles;
using Microsoft.Extensions.Logging;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) => services
                .AddSingleton(provider => provider.GetRequiredService<IConfiguration>().GetSection(MetadataConfiguration.Section).Get<MetadataConfiguration>())
                .AddSingleton<IStreetNameCache, StreetNameCache>()
                .AddScoped<IMetadataUpdater, MetadataUpdater>()
                .AddSingleton(new EnvelopeFactory(
                    WfsContextEventProcessor.EventMapping,
                    new EventDeserializer((eventData, eventType) =>
                        JsonConvert.DeserializeObject(eventData, eventType, WfsContextEventProcessor.SerializerSettings)))
                )
                .AddDbContextFactory<WfsContext>((sp, options) =>
                {
                    var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.WfsProjections);
                    options
                        .UseSqlServer(connectionString,
                            o => o
                                .EnableRetryOnFailure()
                                .UseNetTopologySuite()
                        );
                })
                .AddDbContextFactory<SyndicationContext>((sp, options) =>
                {
                    var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.SyndicationProjections);
                    options
                        .UseSqlServer(connectionString,
                            o => o
                                .EnableRetryOnFailure()
                        );
                })
                .AddSingleton(sp => new ConnectedProjection<WfsContext>[]
                {
                    // new GradeSeparatedJunctionRecordProjection(),
                    new RoadNodeRecordProjection(),
                    new RoadSegmentRecordProjection(sp.GetRequiredService<IStreetNameCache>(), sp.GetRequiredService<UseRoadSegmentSoftDeleteFeatureToggle>())
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
                .AddHostedService<WfsContextEventProcessor>())
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new[]
            {
                WellknownConnectionNames.Events,
                WellknownConnectionNames.WfsProjections,
                WellknownConnectionNames.WfsProjectionsAdmin
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
