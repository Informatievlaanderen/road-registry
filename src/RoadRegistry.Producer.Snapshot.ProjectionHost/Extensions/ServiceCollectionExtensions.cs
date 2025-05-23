namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Extensions;

using System;
using System.Configuration;
using System.Linq;
using BackOffice;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoadNode;
using KafkaProducer = Shared.KafkaProducer;

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
                    var producerOptions = configuration.CreateProducerOptions(entityName + "Topic");

                    return new ConnectedProjection<TSnapshotContext>[]
                    {
                        resolveProjection(sp, new KafkaProducer(producerOptions))
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

    private static ProducerOptions CreateProducerOptions(this IConfiguration configuration, string topicConfigurationKey)
    {
        var bootstrapServers = configuration.GetRequiredValue<string>("Kafka:BootstrapServers");
        var saslUsername = configuration["Kafka:SaslUserName"];
        var saslPassword = configuration["Kafka:SaslPassword"];

        var topic = configuration.GetRequiredValue<string>(topicConfigurationKey);
        var producerOptions = new ProducerOptions(
                new BootstrapServers(bootstrapServers),
                new Topic(topic),
                useSinglePartition: true,
                EventsJsonSerializerSettingsProvider.CreateSerializerSettings())
            .ConfigureEnableIdempotence();

        if (!string.IsNullOrEmpty(saslUsername)
            && !string.IsNullOrEmpty(saslPassword))
        {
            producerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                saslUsername,
                saslPassword));
        }

        return producerOptions;
    }
}
