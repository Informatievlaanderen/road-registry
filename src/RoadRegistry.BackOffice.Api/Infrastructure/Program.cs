namespace RoadRegistry.BackOffice.Api.Infrastructure;

using System;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Be.Vlaanderen.Basisregisters.Api;
using Hosts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Slack;
using Serilog.Sinks.Slack.Models;
using SqlStreamStore;

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
                                ConfigureSerilog = (context, loggerConfiguration) =>
                                {
                                    var slackSinkConfiguation = context.Configuration.GetSection(nameof(SlackSinkOptions));

                                    if (!slackSinkConfiguation.Exists()) return;
                                    var sinkOptions = new SlackSinkOptions
                                    {
                                        CustomUserName = typeof(Program).Namespace,
                                        MinimumLogEventLevel = LogEventLevel.Error
                                    };
                                    slackSinkConfiguation.Bind(sinkOptions);

                                    if(sinkOptions.WebHookUrl is not null) loggerConfiguration.WriteTo.Slack(sinkOptions);
                                }
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

            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.Events);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.Snapshots);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.EditorProjections);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.ProductProjections);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.SyndicationProjections);

            await host.RunAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
