namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Extensions;

using System;
using System.Linq;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoadNode;
using KafkaProducer = Projections.KafkaProducer;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSnapshotProducer<TSnapshotContext, TProjection, TEventProcessor>(this IServiceCollection services,
        string entityName,
        Func<DbContextOptionsBuilder<TSnapshotContext>, TSnapshotContext> resolveContext,
        Func<IServiceProvider, KafkaProducer, TProjection> resolveProjection,
        Func<ConnectedProjection<TSnapshotContext>[], AcceptStreamMessageFilter> buildAcceptStreamMessageFilter
    )
        where TSnapshotContext : RunnerDbContext<TSnapshotContext>
        where TEventProcessor : DbContextEventProcessor<TSnapshotContext>
        where TProjection : ConnectedProjection<TSnapshotContext>
    {
        return services
                .AddSingleton<Func<TSnapshotContext>>(sp =>
                {
                    return () =>
                    {
                        var configuration = sp.GetRequiredService<IConfiguration>();

                        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TSnapshotContext>()
                            .UseSqlServer(
                                configuration.GetConnectionString(WellknownConnectionNames.ProducerSnapshotProjections),
                                options => options
                                    .EnableRetryOnFailure()
                                    .UseNetTopologySuite()
                            );

                        return resolveContext(dbContextOptionsBuilder);
                    };
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
                .AddSingleton(sp => buildAcceptStreamMessageFilter(sp.GetRequiredService<ConnectedProjection<TSnapshotContext>[]>()))
                .AddHostedService<TEventProcessor>()
            ;
    }
}
