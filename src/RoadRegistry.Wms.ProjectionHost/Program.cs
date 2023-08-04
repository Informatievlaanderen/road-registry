namespace RoadRegistry.Wms.ProjectionHost;

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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Projections;
using Schema;

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
                .AddStreetNameCache()
                .AddScoped<IMetadataUpdater, MetadataUpdater>()
                .AddSingleton(new EnvelopeFactory(
                    WmsContextEventProcessor.EventMapping,
                    new EventDeserializer((eventData, eventType) =>
                        JsonConvert.DeserializeObject(eventData, eventType, WmsContextEventProcessor.SerializerSettings)))
                )
                .AddDbContextFactory<WmsContext>((sp, options) =>
                {
                    var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.WmsProjections);
                    options
                        .UseSqlServer(connectionString,
                            o => o
                                .EnableRetryOnFailure()
                                .UseNetTopologySuite()
                        );
                })
                .AddSingleton(sp => new ConnectedProjection<WmsContext>[]
                {
                    new RoadSegmentRecordProjection(sp.GetRequiredService<IStreetNameCache>(), sp.GetRequiredService<UseRoadSegmentSoftDeleteFeatureToggle>()),
                    new RoadSegmentEuropeanRoadAttributeRecordProjection(),
                    new RoadSegmentNationalRoadAttributeRecordProjection()
                })
                .AddSingleton(sp => Resolve.WhenEqualToHandlerMessageType(sp
                    .GetRequiredService<ConnectedProjection<WmsContext>[]>()
                    .SelectMany(projection => projection.Handlers)
                    .ToArray())
                )
                .AddSingleton(sp =>
                    new AcceptStreamMessage<WmsContext>(
                        sp.GetRequiredService<ConnectedProjection<WmsContext>[]>()
                        , WmsContextEventProcessor.EventMapping))
                .AddSingleton<IRunnerDbContextMigratorFactory>(new WmsContextMigrationFactory())
                .AddHostedService<WmsContextEventProcessor>())
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new[]
            {
                WellknownConnectionNames.Events,
                WellknownConnectionNames.WmsProjections,
                WellknownConnectionNames.WmsProjectionsAdmin
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
