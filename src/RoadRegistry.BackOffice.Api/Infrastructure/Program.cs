namespace RoadRegistry.BackOffice.Api.Infrastructure;

using Autofac.Extensions.DependencyInjection;
using Be.Vlaanderen.Basisregisters.Api;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SqlStreamStore;
using System;
using System.Threading.Tasks;

public class Program
{
    public const int HostingPort = 10002;

    protected Program()
    {
    }

    public static IHostBuilder CreateWebHostBuilder(string[] args)
    {
        var webHostBuilder = Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseDefaultForApi<Startup>(
                        new ProgramOptions
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
                                ConfigureSerilog = (context, loggerConfiguration) => loggerConfiguration
                                    .AddSlackSink<Program>(context.Configuration)
                                    .ExcludeCommonErrors()
                            }
                        })
                    .UseKestrel((context, builder) =>
                    {
                        if (context.HostingEnvironment.IsDevelopment())
                        {
                            builder.ListenLocalhost(HostingPort);
                        }
                    });
            });
        return webHostBuilder;
    }

    public static async Task Main(string[] args)
    {
        var host = CreateWebHostBuilder(args).Build();
        var configuration = host.Services.GetRequiredService<IConfiguration>();

        var streamStore = host.Services.GetRequiredService<IStreamStore>();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        try
        {
            await WaitFor.SeqToBecomeAvailable(configuration).ConfigureAwait(false);
            await WaitFor.SqlStreamStoreToBecomeAvailable(streamStore, logger).ConfigureAwait(false);

            logger.LogSqlServerConnectionString(configuration, WellKnownConnectionNames.Events);
            logger.LogSqlServerConnectionString(configuration, WellKnownConnectionNames.Jobs);
            logger.LogSqlServerConnectionString(configuration, WellKnownConnectionNames.Snapshots);
            logger.LogSqlServerConnectionString(configuration, WellKnownConnectionNames.EditorProjections);
            logger.LogSqlServerConnectionString(configuration, WellKnownConnectionNames.ProductProjections);
            logger.LogSqlServerConnectionString(configuration, WellKnownConnectionNames.StreetNameProjections);
            logger.LogSqlServerConnectionString(configuration, WellKnownConnectionNames.SyndicationProjections);

            await host.RunAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
