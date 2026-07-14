namespace RoadRegistry.Projector.Infrastructure;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Keeps the Marten async projections alive without letting a single projection's failures affect the rest of the host.
// When a projection throws, its (isolated) Marten shard is paused while every other shard keeps running. This supervisor
// periodically resumes any paused shard so it recovers on its own once the underlying problem clears. All errors are
// swallowed and logged - it must never bring the host down.
public sealed class MartenProjectionSupervisor : BackgroundService
{
    private readonly MartenProjectionDaemonAccessor _daemonAccessor;
    private readonly TimeSpan _interval;
    private readonly ILogger<MartenProjectionSupervisor> _logger;

    public MartenProjectionSupervisor(
        MartenProjectionDaemonAccessor daemonAccessor,
        IConfiguration configuration,
        ILogger<MartenProjectionSupervisor> logger)
    {
        _daemonAccessor = daemonAccessor;
        _logger = logger;

        var minutes = configuration.GetValue<int?>($"{nameof(MartenProjectionSupervisor)}:RestartIntervalMinutes") ?? 5;
        _interval = TimeSpan.FromMinutes(Math.Clamp(minutes, 1, 60));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, stoppingToken);
                await RestartPausedProjectionsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                // Never let the supervisor take the host down; try again next interval.
                _logger.LogWarning(ex, "Marten projection supervisor tick failed.");
            }
        }
    }

    private async Task RestartPausedProjectionsAsync(CancellationToken cancellationToken)
    {
        var daemon = _daemonAccessor.Daemon;
        if (daemon is null || !daemon.HasAnyPaused())
        {
            return;
        }

        // StartAllAsync resumes every shard that is not already running (i.e. the paused ones) and leaves the running
        // shards untouched, so a single paused projection does not require knowing its shard name. Logged at Error level
        // so a paused projection surfaces on Slack (the Slack sink only forwards Error and above).
        _logger.LogError("A Marten projection shard is paused; attempting to (re)start all paused shards.");
        await daemon.StartAllAsync();
        _logger.LogInformation("Requested (re)start of paused Marten projection shards.");
    }
}
