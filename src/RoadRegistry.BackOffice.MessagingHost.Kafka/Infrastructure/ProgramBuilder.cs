namespace RoadRegistry.BackOffice.MessagingHost.Kafka.Infrastructure;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abstractions;
using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
using Hosts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SqlStreamStore;

public class ProgramBuilder<T> where T : class
{
    public IConfiguration Configuration { get; init; }

    private readonly IHost _host;
    private readonly IStreamStore _streamStore;
    private readonly ILogger<T> _logger;

    private List<Action<IServiceProvider, ILogger<T>>> _configureLoggingActions = new();

    public ProgramBuilder(IHost host)
    {
        Configuration = host.Services.GetRequiredService<IConfiguration>();

        _host = host;
        _streamStore = host.Services.GetRequiredService<IStreamStore>();
        _logger = host.Services.GetRequiredService<ILogger<T>>();
    }

    public ProgramBuilder<T> ConfigureLogging(Action<IServiceProvider, ILogger<T>> configureDelegate)
    {
        _configureLoggingActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
        return this;
    }

    public async Task RunAsync()
    {
        Console.WriteLine($"Starting {typeof(T).FullName}");

        try
        {
            await WaitFor.SeqToBecomeAvailable(Configuration).ConfigureAwait(false);

            _logger.LogSqlServerConnectionString(Configuration, WellknownConnectionNames.Events);
            _logger.LogSqlServerConnectionString(Configuration, WellknownConnectionNames.CommandHost);
            _logger.LogSqlServerConnectionString(Configuration, WellknownConnectionNames.CommandHostAdmin);
            _logger.LogSqlServerConnectionString(Configuration, WellknownConnectionNames.Snapshots);

            foreach (var loggingDelegate in _configureLoggingActions)
                loggingDelegate.Invoke(_host.Services, _logger);

            await DistributedLock<Program>.RunAsync(async () =>
                    {
                        await WaitFor.SqlStreamStoreToBecomeAvailable(_streamStore, _logger).ConfigureAwait(false);
                        await
                            new SqlCommandProcessorPositionStoreSchema(
                                new SqlConnectionStringBuilder(Configuration.GetConnectionString(WellknownConnectionNames.CommandHostAdmin))
                            ).CreateSchemaIfNotExists(WellknownSchemas.CommandHostSchema).ConfigureAwait(false);
                        Console.WriteLine($"Started {typeof(T).FullName}");
                        await _host.RunAsync().ConfigureAwait(false);
                    },
                    DistributedLockOptions.LoadFromConfiguration(Configuration), _logger)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
