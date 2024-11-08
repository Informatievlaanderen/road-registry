namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Extensions;

using BackOffice;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoadNode;
using System;
using System.Linq;
using KafkaProducer = Projections.KafkaProducer;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSnapshotProducer<TSnapshotContext, TProjection, TEventProcessor>(this IServiceCollection services,
        string entityName,
        Func<IServiceProvider, KafkaProducer, TProjection> resolveProjection,
        Func<ConnectedProjection<TSnapshotContext>[], AcceptStreamMessage<TSnapshotContext>> buildAcceptStreamMessage
    )
        where TSnapshotContext : RunnerDbContext<TSnapshotContext>
        where TEventProcessor : RunnerDbContextEventProcessor<TSnapshotContext>
        where TProjection : ConnectedProjection<TSnapshotContext>
    {
        return services
                .AddDbContextFactory<TSnapshotContext>((sp, options) =>
                {
                    var connectionString = sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.ProducerSnapshotProjections);
                    options
                        .UseSqlServer(connectionString,
                        o => o
                            .EnableRetryOnFailure()
                            .UseNetTopologySuite()
                    );
                })
                .AddSingleton(sp =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();

                    return new ConnectedProjection<TSnapshotContext>[]
                    {
                        resolveProjection(sp, new KafkaProducer(new KafkaProducerOptions(
                            configuration["Kafka:BootstrapServers"],
                            configuration["Kafka:SaslUserName"],
                            configuration["Kafka:SaslPassword"],
                            configuration.GetRequiredValue<string>(entityName + "Topic"),
                            true,
                            RoadNodeEventProcessor.SerializerSettings
                        )))
                    };
                })
                .AddSingleton(sp =>
                    Resolve
                        .WhenEqualToHandlerMessageType(
                            sp.GetRequiredService<ConnectedProjection<TSnapshotContext>[]>()
                                .SelectMany(projection => projection.Handlers)
                                .ToArray()
                        )
                )
                .AddSingleton(sp => buildAcceptStreamMessage(sp.GetRequiredService<ConnectedProjection<TSnapshotContext>[]>()))
                .AddHostedService<TEventProcessor>()
            ;
    }
}
