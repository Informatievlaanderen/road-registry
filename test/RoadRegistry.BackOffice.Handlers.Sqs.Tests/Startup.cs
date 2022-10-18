namespace RoadRegistry.BackOffice.Handlers.Sqs.Tests;

using Amazon;
using Autofac;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Startup : TestStartup
{
    public override void ConfigureContainer(ContainerBuilder builder)
    {
    }

    public override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        services.AddSingleton(sp => new SqsOptions("test", "test", RegionEndpoint.EUWest1));
    }
}
