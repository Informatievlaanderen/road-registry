namespace RoadRegistry.Jobs.Processor.Infrastructure;

using System.Threading.Tasks;
using Autofac;
using BackOffice;
using BackOffice.Core;
using BackOffice.Extensions;
using BackOffice.Extracts;
using BackOffice.FeatureCompare;
using BackOffice.Framework;
using BackOffice.Handlers.Sqs;
using BackOffice.ZipArchiveWriters.Cleaning;
using Extracts.Schema;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Hosts.Infrastructure.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
                    .AddSingleton<IBeforeFeatureCompareZipArchiveCleanerFactory, BeforeFeatureCompareZipArchiveCleanerFactory>()
                    .AddRoadNetworkCommandQueue()
                    .RegisterOptions<JobsProcessorOptions>()
                    .AddRoadRegistrySnapshot()
                    .AddScoped(_ => new EventSourcedEntityMap())
                    .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
                    [
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
                    ])))

                    .AddExtractsDbContext(QueryTrackingBehavior.TrackAll)
                    .AddScoped<IExtractRequestCleaner, ExtractRequestCleaner>()

                    .AddHostedService<JobsProcessor>()
                    ;
            })
            .ConfigureContainer((hostContext, builder) =>
            {
                builder
                    .RegisterModule<BlobClientModule>()
                    .RegisterModule<BackOffice.Handlers.MediatorModule>()
                    .RegisterModule<BackOfficeHandlersSqsMediatorModule>()
                    .RegisterModule<SqsHandlersModule>()
                    ;
            })
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings([
                WellKnownConnectionNames.Events,
                WellKnownConnectionNames.Jobs,
                WellKnownConnectionNames.EditorProjections,
                WellKnownConnectionNames.StreetNameProjections
            ])
            .RunAsync();
    }
}
