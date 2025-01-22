namespace RoadRegistry.Wms.ProjectionHost;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
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
using SqlStreamStore;

public class Program
{
    public const int HostingPort = 10010;

    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((_, services) => services
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
                    var connectionString = sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.WmsProjections);
                    options
                        .UseSqlServer(connectionString,
                            o => o
                                .EnableRetryOnFailure()
                                .UseNetTopologySuite()
                        );
                })
                .AddSingleton<IRunnerDbContextMigratorFactory>(new WmsContextMigrationFactory())

                .AddHostedService(sp =>
                {
                   ConnectedProjection<WmsContext>[] connectedProjectionHandlers=
                   [
                       new RoadSegmentRecordProjection(
                            sp.GetRequiredService<IStreetNameCache>(),
                            sp.GetRequiredService<UseRoadSegmentSoftDeleteFeatureToggle>()),
                        new RoadSegmentEuropeanRoadAttributeRecordProjection(),
                        new RoadSegmentNationalRoadAttributeRecordProjection()
                   ];

                   return new WmsContextEventProcessor(
                       sp.GetRequiredService<IStreamStore>(),
                       new AcceptStreamMessage<WmsContext>(
                           connectedProjectionHandlers,
                           WmsContextEventProcessor.EventMapping),
                       sp.GetRequiredService<EnvelopeFactory>(),
                       Resolve.WhenEqualToHandlerMessageType(
                           connectedProjectionHandlers
                           .SelectMany(projection => projection.Handlers)
                           .ToArray()),
                       sp.GetRequiredService<IDbContextFactory<WmsContext>>(),
                       sp.GetRequiredService<Scheduler>(),
                       sp.GetRequiredService<ILoggerFactory>());
                })
                .AddHostedService(sp =>
                {
                   ConnectedProjection<WmsContext>[] connectedProjectionHandlers=
                   [
                        new TransactionZoneRecordProjection(sp.GetRequiredService<ILoggerFactory>())
                   ];

                   return new TransactionZoneEventProcessor(
                       sp.GetRequiredService<IStreamStore>(),
                       new AcceptStreamMessage<WmsContext>(
                           connectedProjectionHandlers,
                           TransactionZoneEventProcessor.EventMapping),
                       sp.GetRequiredService<EnvelopeFactory>(),
                       Resolve.WhenEqualToHandlerMessageType(
                           connectedProjectionHandlers
                           .SelectMany(projection => projection.Handlers)
                           .ToArray()),
                       sp.GetRequiredService<IDbContextFactory<WmsContext>>(),
                       sp.GetRequiredService<Scheduler>(),
                       sp.GetRequiredService<ILoggerFactory>());
                })
            )
            .ConfigureHealthChecks(HostingPort,builder => builder
                .AddHostedServicesStatus()
            )
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings([
                WellKnownConnectionNames.Events,
                WellKnownConnectionNames.WmsProjections,
                WellKnownConnectionNames.WmsProjectionsAdmin
            ])
            .RunAsync(async (sp, _, configuration) =>
            {
                var migratorFactory = sp.GetRequiredService<IRunnerDbContextMigratorFactory>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                await migratorFactory.CreateMigrator(configuration, loggerFactory)
                    .MigrateAsync(CancellationToken.None).ConfigureAwait(false);
            });
    }
}
