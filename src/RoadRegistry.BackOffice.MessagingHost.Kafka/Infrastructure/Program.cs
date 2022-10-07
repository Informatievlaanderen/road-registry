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
    using Uploads;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
                .ConfigureServices((hostContext, services) => services
                    .AddHostedService<Consumer>())
                .ConfigureCommandDispatcher(ConfigureCommandDispatcher)
                .ConfigureContainer((hostContext, builder) => builder
                    .RegisterModule(new Kafka.MediatorModule()))
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
