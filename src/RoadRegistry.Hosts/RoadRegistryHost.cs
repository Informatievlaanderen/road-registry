namespace RoadRegistry.Hosts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Abstractions;
using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SqlStreamStore;

public class RoadRegistryHost<T>
{
    private readonly IConfiguration _configuration;
    private readonly IHost _host;
    private readonly ILogger<T> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IStreamStore _streamStore;
    private readonly List<Action<IServiceProvider, ILogger<T>>> _configureLoggingActions = new();
    private readonly List<string> _wellKnownConnectionNames = new();

    public RoadRegistryHost(IHost host)
    {
        _configuration = host.Services.GetRequiredService<IConfiguration>();
        _host = host;
        _streamStore = host.Services.GetRequiredService<IStreamStore>();
        _logger = host.Services.GetRequiredService<ILogger<T>>();
        _loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
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
            await WaitFor.SeqToBecomeAvailable(_configuration);

            Console.WriteLine($"Starting {ApplicationName}");

            foreach (var wellKnownConnectionName in _wellKnownConnectionNames)
                _logger.LogSqlServerConnectionString(_configuration, wellKnownConnectionName);

            foreach (var loggingDelegate in _configureLoggingActions)
                loggingDelegate.Invoke(_host.Services, _logger);

            await DistributedLock<T>.RunAsync(async () =>
                {
                    await WaitFor.SqlStreamStoreToBecomeAvailable(_streamStore, _logger);
                    await distributedLockCallback(_host.Services, _host, _configuration);

                    Console.WriteLine($"Started {ApplicationName}");
                    await _host.RunAsync().ConfigureAwait(false);
                },
                DistributedLockOptions.LoadFromConfiguration(_configuration), _logger);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
        }
        finally
        {
            await Serilog.Log.CloseAndFlushAsync();
        }
    }

}
