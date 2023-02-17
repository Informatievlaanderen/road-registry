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
                .AddTransient<EventProcessor>()
                .AddSingleton<IStreetNameCache, StreetNameCache>()
                .AddScoped<IMetadataUpdater, MetadataUpdater>()
                .AddSingleton(new EnvelopeFactory(
                    EventProcessor.EventMapping,
                    new EventDeserializer((eventData, eventType) =>
                        JsonConvert.DeserializeObject(eventData, eventType, EventProcessor.SerializerSettings)))
                )
                .AddSingleton(
                    () =>
                        new WfsContext(
                            new DbContextOptionsBuilder<WfsContext>()
                                .UseSqlServer(
                                    hostContext.Configuration.GetConnectionString(WellknownConnectionNames.WfsProjections),
                                    options => options
                                        .EnableRetryOnFailure()
                                        .UseNetTopologySuite()
                                ).Options)
                )
                .AddSingleton(
                    () =>
                        new SyndicationContext(
                            new DbContextOptionsBuilder<SyndicationContext>()
                                .UseSqlServer(
                                    hostContext.Configuration.GetConnectionString(WellknownConnectionNames.SyndicationProjections),
                                    options => options
                                        .EnableRetryOnFailure()
                                ).Options)
                )
                .AddSingleton(sp => new ConnectedProjection<WfsContext>[]
                {
                    // new GradeSeparatedJunctionRecordProjection(),
                    new RoadNodeRecordProjection(),
                    new RoadSegmentRecordProjection(
                        sp.GetRequiredService<IStreetNameCache>())
                })
                .AddSingleton(sp => Resolve.WhenEqualToHandlerMessageType(sp
                    .GetRequiredService<ConnectedProjection<WfsContext>[]>()
                    .SelectMany(projection => projection.Handlers)
                    .ToArray())
                )
                .AddSingleton(sp => AcceptStreamMessage.WhenEqualToMessageType(sp.GetRequiredService<ConnectedProjection<WfsContext>[]>(), EventProcessor.EventMapping))
                .AddSingleton<IRunnerDbContextMigratorFactory>(new WfsContextMigrationFactory()))
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new []
            {
                WellknownConnectionNames.Events,
                WellknownConnectionNames.WfsProjections,
                WellknownConnectionNames.WfsProjectionsAdmin
            })
            .RunAsync(async (sp, host, configuration) =>
            {
                var migratorFactory = host.Services.GetRequiredService<IRunnerDbContextMigratorFactory>();
                var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
                var eventProcessor = sp.GetRequiredService<EventProcessor>();

                await migratorFactory.CreateMigrator(configuration, loggerFactory)
                    .MigrateAsync(CancellationToken.None).ConfigureAwait(false);

                await eventProcessor.Resume(CancellationToken.None);
            });
    }
}
