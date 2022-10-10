namespace RoadRegistry.BackOffice.MessagingHost.Kafka.Infrastructure
{
    using Autofac;
    using Core;
    using Extracts;
    using Framework;
    using Hosts;
    using Hosts.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime;
    using RoadRegistry.BackOffice.MessagingHost.Kafka;
    using RoadRegistry.BackOffice.ZipArchiveWriters.Validation;
    using SqlStreamStore;
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Uploads;

    public class Program
    {
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
                        .AddSingleton(kafkaOptions)
                        .AddSingleton(consumerOptions)
                        .AddHostedService<StreetNameConsumer>();
                })
                .ConfigureCommandDispatcher(ConfigureCommandDispatcher)
                .ConfigureContainer((hostContext, builder, services) =>
                    {
                        var sp = services.BuildServiceProvider();
                        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                        builder
                            .RegisterModule(new Kafka.MediatorModule())
                            .RegisterModule(new ApiModule(hostContext.Configuration, loggerFactory))
                            .RegisterModule(new ConsumerModule(hostContext.Configuration, services, loggerFactory))
                            .RegisterModule(new ProjectorModule(hostContext.Configuration));
                    }
                )
                .Build();

            await roadRegistryHost
                .ConfigureLogging((sp, logger) =>
                {
                    var blobClientOptions = sp.GetRequiredService<BlobClientOptions>();
                    logger.LogBlobClientCredentials(blobClientOptions);
                })
                .RunAsync();
        }

        private static CommandHandlerResolver ConfigureCommandDispatcher(IServiceProvider sp) => Resolve.WhenEqualToMessage(new CommandHandlerModule[]
            {
                new RoadNetworkChangesArchiveCommandModule(
                    sp.GetService<RoadNetworkFeatureCompareBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    new ZipArchiveAfterFeatureCompareValidator(Encoding.GetEncoding(1252)),
                    sp.GetService<IClock>()
                ),
                new RoadNetworkCommandModule(
                    sp.GetService<IStreamStore>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IRoadNetworkSnapshotWriter>(),
                    sp.GetService<IClock>()
                ),
                new RoadNetworkExtractCommandModule(
                    sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    new ZipArchiveAfterFeatureCompareValidator(Encoding.GetEncoding(1252)),
                    sp.GetService<IClock>()
                )
            });
    }
}
