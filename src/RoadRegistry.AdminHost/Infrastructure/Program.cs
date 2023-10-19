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
using BackOffice.ZipArchiveWriters.Validation;
using Consumers;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;
using System.Threading.Tasks;

public class Program
{
    protected Program()
    {
    }
    //TODO-rik zie error logs voor fout on startup, healthcheck?
    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) => services
                .AddEmailClient(hostContext.Configuration)
                .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
                    new CommandHandlerModule[]
                    {
                        new RoadNetworkExtractCommandModule(
                            sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                            sp.GetService<IStreamStore>(),
                            sp.GetService<ILifetimeScope>(),
                            sp.GetService<IRoadNetworkSnapshotReader>(),
                            sp.GetService<IZipArchiveBeforeFeatureCompareValidator>(),
                            sp.GetService<IZipArchiveAfterFeatureCompareValidator>(),
                            sp.GetService<IExtractUploadFailedEmailClient>(),
                            sp.GetService<IClock>(),
                            sp.GetService<ILoggerFactory>()
                        )
                    })))
                .AddSingleton<AdminMessageConsumer>()
                .AddSingleton<ExtractRequestCleanup>()
                .AddSingleton(new ApplicationMetadata(RoadRegistryApplication.BackOffice))
                .AddSingleton<IZipArchiveBeforeFeatureCompareValidator, ZipArchiveBeforeFeatureCompareValidator>()
                .AddSingleton<IZipArchiveAfterFeatureCompareValidator, ZipArchiveAfterFeatureCompareValidator>()
                .AddScoped(_ => new EventSourcedEntityMap())
                .AddStreamStore()
                .AddSingleton<IClock>(SystemClock.Instance)
                .AddRoadNetworkCommandQueue()
                .AddRoadNetworkEventWriter()
                .AddRoadRegistrySnapshot()
                .AddRoadNetworkSnapshotStrategyOptions()
                .AddEditorContext()
                .AddProductContext()
            )
            .ConfigureContainer((hostContext, builder) =>
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

                Task.WaitAll(new[]
                {
                    adminMessageConsumer.ExecuteAsync(stoppingToken),
                    extractRequestCleanup.ExecuteAsync(stoppingToken)
                });
            })
            .Build();

        await roadRegistryHost
            .Log((sp, logger) => {
                logger.LogKnownSqlServerConnectionStrings(roadRegistryHost.Configuration);

                var blobClientOptions = sp.GetRequiredService<BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
            })
            .RunAsync();
    }
}
