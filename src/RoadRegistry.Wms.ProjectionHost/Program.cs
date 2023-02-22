namespace RoadRegistry.Wms.ProjectionHost;

using BackOffice;
using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Hosts.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Newtonsoft.Json;
using NodaTime;
using Projections;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using Schema;
using Serilog;
using Serilog.Debugging;
using SqlStreamStore;
using Syndication.Schema;
using System;
using System.IO;
using System.Linq;
using System.Text;
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
                        new WmsContext(
                            new DbContextOptionsBuilder<WmsContext>()
                                .UseSqlServer(
                                    hostContext.Configuration.GetConnectionString(WellknownConnectionNames.WmsProjections),
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
                .AddSingleton(sp => new ConnectedProjection<WmsContext>[]
                {
                    new RoadSegmentRecordProjection(sp.GetRequiredService<IStreetNameCache>()),
                    new RoadSegmentEuropeanRoadAttributeRecordProjection(),
                    new RoadSegmentNationalRoadAttributeRecordProjection()
                })
                .AddSingleton(sp => Resolve.WhenEqualToHandlerMessageType(sp
                    .GetRequiredService<ConnectedProjection<WmsContext>[]>()
                    .SelectMany(projection => projection.Handlers)
                    .ToArray())
                )
                .AddSingleton(sp => AcceptStreamMessage.WhenEqualToMessageType(sp.GetRequiredService<ConnectedProjection<WmsContext>[]>(), EventProcessor.EventMapping))
                .AddSingleton<IRunnerDbContextMigratorFactory>(new WmsContextMigrationFactory()))
            .ConfigureRunCommand(async sp =>
            {
                var eventProcessor = sp.GetRequiredService<EventProcessor>();
                await eventProcessor.Resume(CancellationToken.None);
            })
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
