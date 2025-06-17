namespace MartenPoc.Projections;

using System.Threading;
using System.Threading.Tasks;
using Marten;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ProjectionBackgroundService : BackgroundService
{
    private readonly IDocumentStore _store;
    private readonly ILogger _logger;

    public ProjectionBackgroundService(IDocumentStore store, ILoggerFactory loggerFactory)
    {
        _store = store;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // This will run all async projections in the background
        _logger.LogInformation("Starting Marten Async Daemon...");

        var daemon = await _store.BuildProjectionDaemonAsync();

        // Optional: start a specific projection shard
        // await daemon.StartShard("YourProjectionName", stoppingToken);

        // Run all async projection shards
        await daemon.StartAllAsync();

        _logger.LogInformation("Stopping Marten Async Daemon...");
        await daemon.StopAllAsync();
    }
}
