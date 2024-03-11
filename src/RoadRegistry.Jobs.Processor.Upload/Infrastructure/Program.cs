namespace RoadRegistry.Jobs.Processor.Upload.Infrastructure;

using BackOffice.Framework;
using Hosts;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Extensions;
using Hosts.Infrastructure.Extensions;
using Autofac;
using BackOffice.Core;
using BackOffice.Extracts;
using BackOffice.Uploads;
using BackOffice.ZipArchiveWriters.Cleaning;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
using Hosts.Infrastructure.Modules;
using Microsoft.Extensions.Logging;
using NodaTime;
using Options;
using SqlStreamStore;

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
                services
                    .AddSingleton(new ApplicationMetadata(RoadRegistryApplication.BackOffice))
                    .AddHttpClient()
                    .AddTicketing()
                    .AddDistributedStreamStoreLockOptions()
                    .AddRoadNetworkDbIdGenerator()
                    .AddEditorContext()
                    .AddStreetNameCache()
                    .AddJobsContext()
                    .AddFeatureCompare()
                    .AddSingleton<IBeforeFeatureCompareZipArchiveCleaner, BeforeFeatureCompareZipArchiveCleaner>()
                    .AddRoadNetworkCommandQueue()
                    .RegisterOptions<UploadProcessorOptions>()
                    .AddRoadRegistrySnapshot()
                    .AddScoped(_ => new EventSourcedEntityMap())
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

                    .AddHostedService<UploadProcessor>()
                    ;
            })
            .ConfigureContainer((hostContext, builder) =>
            {
                builder
                    .RegisterModule(new DataDogModule(hostContext.Configuration))
                    .RegisterModule<BlobClientModule>()
                    .RegisterModule<BackOffice.Handlers.MediatorModule>();
            })
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new[] {
                WellKnownConnectionNames.Events,
                WellKnownConnectionNames.Jobs,
                WellKnownConnectionNames.EditorProjections,
                WellKnownConnectionNames.StreetNameProjections
            })
            .RunAsync();
    }
}
