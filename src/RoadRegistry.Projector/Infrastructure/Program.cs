namespace RoadRegistry.Projector.Infrastructure;

using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
using Microsoft.AspNetCore.Hosting;
using RoadRegistry.Hosts.Infrastructure.Extensions;

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
    {
        new WebHostBuilder()
            .UseDefaultForApi<Startup>(options)
            .RunWithLock<Program>();
    }
}
