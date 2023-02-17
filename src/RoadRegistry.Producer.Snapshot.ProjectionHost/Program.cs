namespace RoadRegistry.Producer.Snapshot.ProjectionHost
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Extensions;
    using GradeSeparatedJunction;
    using Hosts;
    using Hosts.Metadata;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NationalRoad;
    using Newtonsoft.Json;
    using RoadNode;
    using RoadSegment;
    using RoadSegmentSurface;
    using Syndication.Schema;
    using System;
    using System.Linq;
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
                    .AddSingleton<IStreetNameCache, StreetNameCache>()
                    .AddSingleton(new EnvelopeFactory(
                        RoadNodeEventProcessor.EventMapping,
                        new EventDeserializer((eventData, eventType) =>
                            JsonConvert.DeserializeObject(eventData, eventType, RoadNodeEventProcessor.SerializerSettings)))
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
                    .AddSnapshotProducer<RoadNodeProducerSnapshotContext, RoadNodeRecordProjection, RoadNodeEventProcessor>(
                        "RoadNode",
                        dbContextOptionsBuilder => new RoadNodeProducerSnapshotContext(dbContextOptionsBuilder.Options),
                        (_, kafkaProducer) => new RoadNodeRecordProjection(kafkaProducer),
                        connectedProjection => RoadNodeAcceptStreamMessage.WhenEqualToMessageType(connectedProjection, RoadNodeEventProcessor.EventMapping)
                    )
                    .AddSnapshotProducer<RoadSegmentProducerSnapshotContext, RoadSegmentRecordProjection, RoadSegmentEventProcessor>(
                        "RoadSegment",
                        dbContextOptionsBuilder => new RoadSegmentProducerSnapshotContext(dbContextOptionsBuilder.Options),
                        (sp, kafkaProducer) => new RoadSegmentRecordProjection(kafkaProducer, sp.GetRequiredService<IStreetNameCache>()),
                        connectedProjection => RoadSegmentAcceptStreamMessage.WhenEqualToMessageType(connectedProjection, RoadSegmentEventProcessor.EventMapping)
                    )
                    .AddSnapshotProducer<NationalRoadProducerSnapshotContext, NationalRoadRecordProjection, NationalRoadEventProcessor>(
                        "NationalRoad",
                        dbContextOptionsBuilder => new NationalRoadProducerSnapshotContext(dbContextOptionsBuilder.Options),
                        (_, kafkaProducer) => new NationalRoadRecordProjection(kafkaProducer),
                        connectedProjection => NationalRoadAcceptStreamMessage.WhenEqualToMessageType(connectedProjection, NationalRoadEventProcessor.EventMapping)
                    )
                    .AddSnapshotProducer<GradeSeparatedJunctionProducerSnapshotContext, GradeSeparatedJunctionRecordProjection, GradeSeparatedJunctionEventProcessor>(
                        "GradeSeparatedJunction",
                        dbContextOptionsBuilder => new GradeSeparatedJunctionProducerSnapshotContext(dbContextOptionsBuilder.Options),
                        (_, kafkaProducer) => new GradeSeparatedJunctionRecordProjection(kafkaProducer),
                        connectedProjection => GradeSeparatedJunctionAcceptStreamMessage.WhenEqualToMessageType(connectedProjection, GradeSeparatedJunctionEventProcessor.EventMapping)
                    )
                    .AddSnapshotProducer<RoadSegmentSurfaceProducerSnapshotContext, RoadSegmentSurfaceRecordProjection, RoadSegmentSurfaceEventProcessor>(
                        "RoadSegmentSurface",
                        dbContextOptionsBuilder => new RoadSegmentSurfaceProducerSnapshotContext(dbContextOptionsBuilder.Options),
                        (_, kafkaProducer) => new RoadSegmentSurfaceRecordProjection(kafkaProducer),
                        connectedProjection => RoadSegmentSurfaceAcceptStreamMessage.WhenEqualToMessageType(connectedProjection, RoadSegmentSurfaceEventProcessor.EventMapping)
                    )
                    .AddSingleton(typeof(Program).Assembly
                        .GetTypes()
                        .Where(x => !x.IsAbstract && typeof(IRunnerDbContextMigratorFactory).IsAssignableFrom(x))
                        .Select(type => (IRunnerDbContextMigratorFactory)Activator.CreateInstance(type))
                        .ToArray()))
                .Build();

            await roadRegistryHost
                .LogSqlServerConnectionStrings(new []
                {
                    WellknownConnectionNames.SyndicationProjections,
                    WellknownConnectionNames.Events,
                    WellknownConnectionNames.ProducerSnapshotProjections,
                    WellknownConnectionNames.ProducerSnapshotProjectionsAdmin
                })
                .RunAsync(async (sp, Hosts, configuration) =>
                {
                    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                    var migratorFactories = sp.GetRequiredService<IRunnerDbContextMigratorFactory[]>();

                    foreach (var migratorFactory in migratorFactories)
                    {
                        await migratorFactory
                            .CreateMigrator(configuration, loggerFactory)
                            .MigrateAsync(CancellationToken.None).ConfigureAwait(false);
                    }
                });
        }
    }
}
