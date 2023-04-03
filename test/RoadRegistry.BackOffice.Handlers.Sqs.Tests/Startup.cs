namespace RoadRegistry.BackOffice.Handlers.Sqs.Tests;

using Autofac;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Startup : TestStartup
{
    protected override void ConfigureContainer(ContainerBuilder builder)
    {
    }

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        services.AddSingleton<SqsOptions>(sp => new FakeSqsOptions());
    }
}
