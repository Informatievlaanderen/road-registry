namespace RoadRegistry.AdminHost.Infrastructure;

using Autofac;
using BackOffice;
using BackOffice.Configuration;
using BackOffice.Core;
using BackOffice.Extensions;
using BackOffice.Extracts;
using BackOffice.Framework;
using BackOffice.Handlers.Sqs;
using BackOffice.Uploads;
using Consumers;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using Options;
using SqlStreamStore;
using System.Threading.Tasks;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((_, services) => services
                .AddTicketing()
                .AddEmailClient()
                .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
                    new CommandHandlerModule[]
                    {
                        new RoadNetworkExtractCommandModule(
                            sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                            sp.GetService<IStreamStore>(),
                            sp.GetService<ILifetimeScope>(),
                            sp.GetService<IRoadNetworkSnapshotReader>(),
                            sp.GetService<IZipArchiveBeforeFeatureCompareValidator>(),
                            sp.GetService<IExtractUploadFailedEmailClient>(),
                            sp.GetService<IClock>(),
                            sp.GetService<ILoggerFactory>()
                        )
                    })))
                .RegisterOptions<AdminHostOptions>()
                .AddSingleton<AdminMessageConsumer>()
                .AddSingleton<ExtractRequestCleanup>()
                .AddSingleton(new ApplicationMetadata(RoadRegistryApplication.BackOffice))
                .AddOrganizationCache()
                .AddStreetNameCache()
                .AddFeatureCompare()
                .AddScoped(_ => new EventSourcedEntityMap())
                .AddStreamStore()
                .AddSingleton<IClock>(SystemClock.Instance)
                .AddRoadNetworkCommandQueue()
                .AddRoadNetworkEventWriter()
                .AddRoadRegistrySnapshot()
                .AddEditorContext()
                .AddProductContext()
            )
            .ConfigureContainer((_, builder) =>
            {
                builder.RegisterModule<MediatorModule>();
                builder.RegisterModule<BackOffice.Handlers.MediatorModule>();
                builder.RegisterModule<Snapshot.Handlers.MediatorModule>();
                builder.RegisterModule<Snapshot.Handlers.Sqs.MediatorModule>();

                builder.RegisterModule<ContextModule>();
                builder.RegisterModule<SqsHandlersModule>();
            })
            .ConfigureRunCommand(async (sp, stoppingToken) =>
            {
                var adminMessageConsumer = sp.GetRequiredService<AdminMessageConsumer>();
                var extractRequestCleanup = sp.GetRequiredService<ExtractRequestCleanup>();

                await Task.WhenAll(
                    adminMessageConsumer.ExecuteAsync(stoppingToken),
                    extractRequestCleanup.ExecuteAsync(stoppingToken)
                );
            })
            .Build();

        await roadRegistryHost
            .Log((sp, logger) => {
                logger.LogKnownSqlServerConnectionStrings(roadRegistryHost.Configuration);

                var blobClientOptions = sp.GetRequiredService<BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
            })
            .RunAsync((sp, _, _) =>
            {
                var adminHostOptions = sp.GetRequiredService<AdminHostOptions>();
                if (adminHostOptions.AlwaysRunning)
                {
                    var logger = sp.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Host is configured to be always running.");
                }

                return Task.CompletedTask;
            });
    }
}
