namespace RoadRegistry.Hosts;

using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlStreamStore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class RoadRegistryHost<T>
{
    public IConfiguration Configuration { get; }

    private readonly IHost _host;
    private readonly IHealthCheck[] _healthChecks;
    private readonly ILogger<T> _logger;
    private readonly IStreamStore _streamStore;
    private readonly List<Action<IServiceProvider, ILogger<T>>> _configureLoggingActions = new();
    private readonly List<string> _wellKnownConnectionNames = new();
    private readonly Func<IServiceProvider, Task> _runCommandDelegate;

    public RoadRegistryHost(IHost host, Func<IServiceProvider, Task> runCommandDelegate, params IHealthCheck[] healthChecks)
    {
        Configuration = host.Services.GetRequiredService<IConfiguration>();
        _host = host;
        _healthChecks = healthChecks;
        _streamStore = host.Services.GetRequiredService<IStreamStore>();
        _logger = host.Services.GetRequiredService<ILogger<T>>();
        _runCommandDelegate = runCommandDelegate ?? ((sp) => _host.RunAsync());
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
                    await _runCommandDelegate(_host.Services).ConfigureAwait(false);
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
}
