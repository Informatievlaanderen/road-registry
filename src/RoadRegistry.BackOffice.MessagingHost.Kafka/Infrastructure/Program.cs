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
    using System.Text;
    using System.Threading.Tasks;
    using Uploads;
    using MediatorModule = Kafka.MediatorModule;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new RoadRegistryHostBuilder(args)
                .ConfigureHostConfiguration(builder =>
                {
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                })
                .ConfigureLogging((hostContext, builder) =>
                {
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddHostedService<Consumer>();
                })
                .ConfigureCommandDispatcher(sp => Resolve.WhenEqualToMessage(
                    new CommandHandlerModule[]
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
                    })
                )
                .ConfigureContainer((hostContext, builder) => builder
                    .RegisterModule(new MediatorModule())
                )
                .Build();

            await new ProgramBuilder<Program>(host)
                .ConfigureLogging((sp, logger) =>
                {
                    var blobClientOptions = sp.GetRequiredService<BlobClientOptions>();
                    logger.LogBlobClientCredentials(blobClientOptions);
                })
                .RunAsync();
        }
    }
}
