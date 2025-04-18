namespace RoadRegistry.Jobs.Processor.Infrastructure;

using Autofac;
using BackOffice;
using BackOffice.Core;
using BackOffice.Extensions;
using BackOffice.Extracts;
using BackOffice.Framework;
using BackOffice.Uploads;
using BackOffice.ZipArchiveWriters.Cleaning;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Hosts.Infrastructure.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using Options;
using SqlStreamStore;
using System.Threading.Tasks;
using BackOffice.FeatureCompare;
using BackOffice.ZipArchiveWriters.Cleaning.V1;

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
                    .AddSingleton<IBeforeFeatureCompareZipArchiveCleanerFactory, BeforeFeatureCompareZipArchiveCleanerFactory>()
                    .AddRoadNetworkCommandQueue()
                    .RegisterOptions<JobsProcessorOptions>()
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
                                sp.GetService<IZipArchiveBeforeFeatureCompareValidatorFactory>(),
                                sp.GetService<IExtractUploadFailedEmailClient>(),
                                sp.GetService<IClock>(),
                                sp.GetService<ILoggerFactory>()
                            )
                        })))

                    .AddHostedService<JobsProcessor>()
                    ;
            })
            .ConfigureContainer((hostContext, builder) =>
            {
                builder
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
