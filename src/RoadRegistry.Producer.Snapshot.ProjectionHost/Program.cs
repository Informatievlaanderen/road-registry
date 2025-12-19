namespace RoadRegistry.Producer.Snapshot.ProjectionHost
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Extensions;
    using GradeSeparatedJunction;
    using Hosts;
    using Hosts.Infrastructure.Extensions;
    using Hosts.Metadata;
    using Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NationalRoad;
    using Newtonsoft.Json;
    using RoadNode;
    using RoadSegment;
    using RoadSegmentSurface;

    public class Program
    {
        public const int HostingPort = 10016;

        protected Program()
        {
        }

        public static async Task Main(string[] args)
        {
            var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
                .ConfigureServices((hostContext, services) => services
                    .AddSingleton(provider => provider.GetRequiredService<IConfiguration>().GetSection(MetadataConfiguration.Section).Get<MetadataConfiguration>())
                    .AddStreetNameCache()
                    .AddSingleton(new EnvelopeFactory(
                        RoadNodeEventProcessor.EventMapping,
                        new EventDeserializer((eventData, eventType) =>
                            JsonConvert.DeserializeObject(eventData, eventType, RoadNodeEventProcessor.SerializerSettings)))
                    )
                    .AddSnapshotProducer<RoadNodeProducerSnapshotContext, RoadNodeRecordProjection, RoadNodeEventProcessor>(
                        "RoadNode",
                        (_, kafkaProducer) => new RoadNodeRecordProjection(kafkaProducer),
                        connectedProjection => new AcceptStreamMessage<RoadNodeProducerSnapshotContext>(connectedProjection, RoadNodeEventProcessor.EventMapping)
                    )
                    .AddSnapshotProducer<RoadSegmentProducerSnapshotContext, RoadSegmentRecordProjection, RoadSegmentEventProcessor>(
                        "RoadSegment",
                        (sp, kafkaProducer) => new RoadSegmentRecordProjection(kafkaProducer, sp.GetRequiredService<IStreetNameCache>()),
                        connectedProjection => new AcceptStreamMessage<RoadSegmentProducerSnapshotContext>(connectedProjection, RoadSegmentEventProcessor.EventMapping)
                    )
                    .AddSnapshotProducer<NationalRoadProducerSnapshotContext, NationalRoadRecordProjection, NationalRoadEventProcessor>(
                        "NationalRoad",
                        (_, kafkaProducer) => new NationalRoadRecordProjection(kafkaProducer),
                        connectedProjection => new AcceptStreamMessage<NationalRoadProducerSnapshotContext>(connectedProjection, NationalRoadEventProcessor.EventMapping)
                    )
                    .AddSnapshotProducer<GradeSeparatedJunctionProducerSnapshotContext, GradeSeparatedJunctionRecordProjection, GradeSeparatedJunctionEventProcessor>(
                        "GradeSeparatedJunction",
                        (_, kafkaProducer) => new GradeSeparatedJunctionRecordProjection(kafkaProducer),
                        connectedProjection => new AcceptStreamMessage<GradeSeparatedJunctionProducerSnapshotContext>(connectedProjection, GradeSeparatedJunctionEventProcessor.EventMapping)
                    )
                    .AddSnapshotProducer<RoadSegmentSurfaceProducerSnapshotContext, RoadSegmentSurfaceRecordProjection, RoadSegmentSurfaceEventProcessor>(
                        "RoadSegmentSurface",
                        (_, kafkaProducer) => new RoadSegmentSurfaceRecordProjection(kafkaProducer),
                        connectedProjection => new AcceptStreamMessage<RoadSegmentSurfaceProducerSnapshotContext>(connectedProjection, RoadSegmentSurfaceEventProcessor.EventMapping)
                    )
                    .AddSingleton(typeof(Program).Assembly
                        .GetTypes()
                        .Where(x => !x.IsAbstract && typeof(IRunnerDbContextMigratorFactory).IsAssignableFrom(x))
                        .Select(type => (IRunnerDbContextMigratorFactory)Activator.CreateInstance(type))
                        .ToArray()))
                .ConfigureHealthChecks(HostingPort, builder => builder
                    .AddHostedServicesStatus()
                )
                .Build();

            await roadRegistryHost
                .LogSqlServerConnectionStrings([
                    WellKnownConnectionNames.Events,
                    WellKnownConnectionNames.ProducerSnapshotProjections,
                    WellKnownConnectionNames.ProducerSnapshotProjectionsAdmin
                ])
                .RunAsync(async (sp, host, configuration) =>
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
