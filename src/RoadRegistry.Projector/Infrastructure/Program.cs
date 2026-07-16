namespace RoadRegistry.Projector.Infrastructure;

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Projector.Infrastructure.Modules;

public class Program
{
    public const int HostingPort = 10006;

    protected Program()
    { }

    public static void Main(string[] args)
    {
        Run(new ProgramOptions
        {
            Hosting =
            {
                HttpPort = HostingPort
            },
            Logging =
            {
                WriteTextToConsole = false,
                WriteJsonToConsole = false
            },
            Runtime =
            {
                CommandLineArgs = args
            },
            MiddlewareHooks =
            {
                ConfigureDistributedLock = DistributedLockOptions.LoadFromConfiguration,
                ConfigureSerilog = (context, loggerConfiguration) => loggerConfiguration
                    .AddSlackSink<Program>(context.Configuration)
                    .ExcludeCommonErrors()
            }
        });
    }

    private static void Run(ProgramOptions options)
        => new HostBuilder()
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>((hostContext, containerBuilder) =>
            {
                var services = new ServiceCollection();
                containerBuilder.RegisterModule(new ApiModule(hostContext.Configuration, services));
            })
            .UseDefaultForApi<Startup>(options)
            .RunWithLock<Program>();
}
