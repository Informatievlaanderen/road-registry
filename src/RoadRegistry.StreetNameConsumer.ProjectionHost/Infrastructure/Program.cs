namespace RoadRegistry.StreetNameConsumer.ProjectionHost.Infrastructure;

using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.Projector.Modules;
using Hosts;
using Microsoft.Extensions.DependencyInjection;
using Modules;
using System;
using System.Threading.Tasks;
using Schema;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;
                var bootstrapServers = configuration["Kafka:BootstrapServers"];
                var kafkaOptions = new KafkaOptions(bootstrapServers, configuration["Kafka:SaslUserName"], configuration["Kafka:SaslPassword"], EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

                var topic = configuration["Kafka:Consumers:StreetName:Topic"] ?? throw new ArgumentException("Configuration has no StreetName Consumer with a Topic.");
                var consumerGroupSuffix = configuration["Kafka:Consumers:StreetName:GroupSuffix"];
                var consumerOptions = new ConsumerOptions(topic, consumerGroupSuffix);

                services
                    .AddSingleton<IRunnerDbContextMigratorFactory>(new StreetNameConsumerContextMigrationFactory())
                    .AddSingleton(kafkaOptions)
                    .AddSingleton(consumerOptions)
                    .AddHostedService<StreetNameConsumer>();
            })
            .ConfigureContainer((hostContext, builder) =>
                {
                    builder
                        .RegisterModule<ConsumerModule>()
                        .RegisterModule(new ApiModule(hostContext.Configuration))
                        .RegisterModule(new ProjectorModule(hostContext.Configuration));
                }
            )
            .Build();

        await roadRegistryHost
            .RunAsync();
    }
}
