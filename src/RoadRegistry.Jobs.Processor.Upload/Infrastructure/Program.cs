namespace RoadRegistry.Jobs.Processor.Upload.Infrastructure;

using BackOffice.Framework;
using Hosts;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using BackOffice.Extensions;
using Hosts.Infrastructure.Extensions;

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
                    .AddJobsContext()
                    .AddFeatureCompare()
                    //TODO-rik cleanup
                    //.AddScoped(_ => new EventSourcedEntityMap())
                    //.AddRoadNetworkCommandQueue()
                    //.AddRoadNetworkEventWriter()
                    //.AddEditorContext()

                    .AddHostedService<UploadProcessor>()
                    ;
            })
            .ConfigureContainer((hostContext, builder) => { })
            .Build();

        await roadRegistryHost
            .RunAsync();
    }
}
