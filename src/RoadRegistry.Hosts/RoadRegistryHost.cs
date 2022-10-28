namespace RoadRegistry.Hosts;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackOffice.Abstractions;
using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
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
    private readonly IStreamStore _streamStore;
    private readonly List<Action<IServiceProvider, ILogger<T>>> _configureLoggingActions = new();

    public RoadRegistryHost(IHost host)
    {
        _configuration = host.Services.GetRequiredService<IConfiguration>();
        _host = host;
        _streamStore = host.Services.GetRequiredService<IStreamStore>();
        _logger = host.Services.GetRequiredService<ILogger<T>>();
    }

    public RoadRegistryHost<T> ConfigureLogging(Action<IServiceProvider, ILogger<T>> configureDelegate)
    {
        _configureLoggingActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
        return this;
    }

    public async Task RunAsync()
    {
        try
        {
            await WaitFor.SeqToBecomeAvailable(_configuration);

            _logger.LogSqlServerConnectionString(_configuration, WellknownConnectionNames.Events);
            _logger.LogSqlServerConnectionString(_configuration, WellknownConnectionNames.CommandHost);
            _logger.LogSqlServerConnectionString(_configuration, WellknownConnectionNames.CommandHostAdmin);
            _logger.LogSqlServerConnectionString(_configuration, WellknownConnectionNames.Snapshots);

            foreach (var loggingDelegate in _configureLoggingActions)
                loggingDelegate.Invoke(_host.Services, _logger);

            await DistributedLock<T>.RunAsync(async () =>
                {
                    await WaitFor.SqlStreamStoreToBecomeAvailable(_streamStore, _logger);
                    await
                        new SqlCommandProcessorPositionStoreSchema(
                            new SqlConnectionStringBuilder(_configuration.GetConnectionString(WellknownConnectionNames.CommandHostAdmin))
                        ).CreateSchemaIfNotExists(WellknownSchemas.CommandHostSchema);
                    await _host.RunAsync();
                },
                DistributedLockOptions.LoadFromConfiguration(_configuration), _logger);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}