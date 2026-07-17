namespace RoadRegistry.Projector.Infrastructure;

using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BackOffice;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Projector.Infrastructure.Modules;
using Serilog;
using LoggerConfigurationExtensions = RoadRegistry.Hosts.Infrastructure.Extensions.LoggerConfigurationExtensions;

public class Program
{
    public const int HostingPort = 10006;

    protected Program()
    {
    }

    public static IHostBuilder CreateWebHostBuilder(string[] args)
    {
        var webHostBuilder = Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>((hostContext, containerBuilder) =>
            {
                var services = new ServiceCollection();
                containerBuilder.RegisterModule(new ApiModule(hostContext.Configuration, services));
            })
            .UseDefaultForApi<Startup>(
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
                        ConfigureDistributedLock = DistributedLockOptions.LoadFromConfiguration,
                        ConfigureSerilog = (context, loggerConfiguration) => loggerConfiguration
                            .AddSlackSink<Program>(context.Configuration)
                            .ExcludeCommonErrors()
                    }
                });
        return webHostBuilder;
    }

    public static async Task Main(string[] args)
    {
        using var host = CreateWebHostBuilder(args).Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        try
        {
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();

            await DistributedLock<Program>.RunAsync(async () =>
                {
                    Console.WriteLine($"Started {typeof(Program).Namespace}");

                    foreach (var migratorFactory in host.Services.GetServices<IDbMigratorFactory>())
                    {
                        var migrator = migratorFactory.CreateMigrator(configuration, loggerFactory);
                        await migrator.MigrateAsync(CancellationToken.None).ConfigureAwait(false);
                    }

                    var stoppingToken = host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;
                    await host.RunAsync(stoppingToken).ConfigureAwait(false);
                },
                DistributedLockOptions.LoadFromConfiguration(configuration), logger);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
        }
        finally
        {
            await Task.Delay(LoggerConfigurationExtensions.SlackSinkPeriod);
            await Log.CloseAndFlushAsync();

            // Allow some time for flushing before shutdown.
            await Task.Delay(500, CancellationToken.None);
        }
    }
}
