namespace RoadRegistry.Jobs.Processor.Upload.Infrastructure;

using BackOffice.Framework;
using Hosts;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Extensions;
using Hosts.Infrastructure.Extensions;
using Autofac;
using BackOffice.ZipArchiveWriters.Cleaning;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
using Hosts.Infrastructure.Modules;

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
                    //.AddScoped(_ => new EventSourcedEntityMap())
                    .AddDistributedStreamStoreLockOptions()
                    .AddRoadNetworkDbIdGenerator()
                    .AddEditorContext()
                    //.AddOrganizationCache()
                    .AddStreetNameCache()
                    .AddJobsContext()
                    .AddFeatureCompare()
                    .AddSingleton<IBeforeFeatureCompareZipArchiveCleaner, BeforeFeatureCompareZipArchiveCleaner>()
                    //TODO-rik cleanup
                    .AddRoadNetworkCommandQueue()
                    //.AddRoadNetworkEventWriter()

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
                WellKnownConnectionNames.JobsAdmin
            })
            .RunAsync();
    }
}
