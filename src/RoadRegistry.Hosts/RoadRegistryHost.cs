namespace RoadRegistry.Hosts;

using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlStreamStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Extensions;
using BackOffice.FeatureToggles;
using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

public class RoadRegistryHost<T>
{
    public IConfiguration Configuration { get; }

    private readonly IHost _host;
    private readonly List<Action<HealthCheckInitializer>> _configureHealthCheckActions;
    private readonly ILogger<T> _logger;
    private readonly IStreamStore _streamStore;
    private readonly List<Action<IServiceProvider, ILogger<T>>> _configureLoggingActions = new();
    private readonly List<string> _wellKnownConnectionNames = new();
    private readonly Func<IServiceProvider, Task> _runCommandDelegate;

    public RoadRegistryHost(IHost host, Func<IServiceProvider, Task> runCommandDelegate, List<Action<HealthCheckInitializer>> configureHealthCheckActions)
    {
        Configuration = host.Services.GetRequiredService<IConfiguration>();
        _host = host;
        _configureHealthCheckActions = configureHealthCheckActions;
        _streamStore = host.Services.GetRequiredService<IStreamStore>();
        _logger = host.Services.GetRequiredService<ILogger<T>>();
        _runCommandDelegate = runCommandDelegate ?? (_ => _host.RunAsync());
    }

    public string ApplicationName => typeof(T).Namespace;

    public RoadRegistryHost<T> Log(Action<IServiceProvider, ILogger<T>> configureDelegate)
    {
        _configureLoggingActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
        return this;
    }

    public RoadRegistryHost<T> LogSqlServerConnectionStrings(string[] wellKnownConnectionNames)
    {
        _wellKnownConnectionNames.AddRange(wellKnownConnectionNames);
        return this;
    }

    public Task RunAsync() => RunAsync((_, _, _) => Task.CompletedTask);

    public async Task RunAsync(Func<IServiceProvider, IHost, IConfiguration, Task> distributedLockCallback)
    {
        try
        {
            await WaitFor.SeqToBecomeAvailable(Configuration);

            Console.WriteLine($"Starting {ApplicationName}");

            foreach (var wellKnownConnectionName in _wellKnownConnectionNames)
                _logger.LogSqlServerConnectionString(Configuration, wellKnownConnectionName);

            foreach (var loggingDelegate in _configureLoggingActions)
                loggingDelegate.Invoke(_host.Services, _logger);

            var environment = _host.Services.GetRequiredService<IHostEnvironment>();
            if (environment.IsDevelopment())
            {
                await _host.Services.CreateMissingBucketsAsync(CancellationToken.None).ConfigureAwait(false);
                await _host.Services.CreateMissingQueuesAsync(CancellationToken.None).ConfigureAwait(false);
            }

            using (var scope = _host.Services.CreateScope())
            {
                var optionsValidator = scope.ServiceProvider.GetRequiredService<OptionsValidator>();
                optionsValidator.ValidateAndThrow();
            }

            await DistributedLock<T>.RunAsync(async () =>
                {
                    await WaitFor.SqlStreamStoreToBecomeAvailable(_streamStore, _logger);
                    await distributedLockCallback(_host.Services, _host, Configuration);

                    Console.WriteLine($"Started {ApplicationName}");

                    var runTasks = new List<Task>
                    {
                        _runCommandDelegate(_host.Services)
                    };

                    if (_configureHealthCheckActions.Any())
                    {
                        runTasks.Add(RunHealthChecksWebApp());
                    }
                    
                    Task.WaitAll(runTasks.ToArray());
                },
                DistributedLockOptions.LoadFromConfiguration(Configuration), _logger);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
        }
        finally
        {
            await Serilog.Log.CloseAndFlushAsync();
        }

        _logger.LogInformation($"Stopped {ApplicationName}");
    }

    private async Task RunHealthChecksWebApp()
    {
        var environment = _host.Services.GetRequiredService<IHostEnvironment>();
        var isDevelopment = environment.IsDevelopment();

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = ApplicationName
        });

        builder.Configuration.AddConfiguration(_host.Services.GetRequiredService<IConfiguration>());

        var healthChecksBuilder = builder.Services
            .AddHealthChecks();

        builder.WebHost.ConfigureServices((hostContext, _) =>
            {
                var useHealthChecksFeatureToggle = hostContext.Configuration.GetFeatureToggles<ApplicationFeatureToggle>().OfType<UseHealthChecksFeatureToggle>().Single();
                if (useHealthChecksFeatureToggle.FeatureEnabled)
                {
                    var healthCheckInitializer = HealthCheckInitializer.Configure(healthChecksBuilder, hostContext.Configuration, isDevelopment);

                    foreach (var configureHealthCheckAction in _configureHealthCheckActions)
                    {
                        configureHealthCheckAction?.Invoke(healthCheckInitializer);
                    }
                }
            });

        var webApp = builder.Build();
        webApp
            .UseRouting()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
            });

        await webApp.RunAsync();
    }
}
