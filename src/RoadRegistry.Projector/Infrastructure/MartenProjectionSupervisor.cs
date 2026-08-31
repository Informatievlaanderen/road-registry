namespace RoadRegistry.Projector.Infrastructure;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JasperFx;
using JasperFx.Events.Daemon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Keeps the Marten async projections alive without letting a single projection's failures affect the rest of the host.
// When a projection throws, its (isolated) Marten shard is paused while every other shard keeps running. This supervisor
// periodically restarts any shard that is not running while it is supposed to be - paused after an error, or merely
// stopped - so it recovers on its own once the underlying problem clears. All errors are swallowed and logged: it must
// never bring the host down.
//
// It only resumes shards whose desired state says they should be running. A projection an operator stopped is left
// alone: previously this used StartAllAsync, which resumed every shard that was not running and so undid a deliberate
// stop within the restart interval - including during maintenance on that very projection.
public sealed class MartenProjectionSupervisor : BackgroundService
{
    private readonly MartenProjectionDaemonAccessor _daemonAccessor;
    private readonly IReadOnlyList<ProjectionDetail> _martenProjections;
    private readonly MartenProjectionDesiredStateStore _desiredStateStore;
    private readonly TimeSpan _interval;
    private readonly ILogger<MartenProjectionSupervisor> _logger;

    public MartenProjectionSupervisor(
        MartenProjectionDaemonAccessor daemonAccessor,
        IReadOnlyList<ProjectionDetail> martenProjections,
        MartenProjectionDesiredStateStore desiredStateStore,
        IConfiguration configuration,
        ILogger<MartenProjectionSupervisor> logger)
    {
        _daemonAccessor = daemonAccessor;
        _martenProjections = martenProjections;
        _desiredStateStore = desiredStateStore;
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
                await RestartProjectionsThatShouldBeRunningAsync(stoppingToken);
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

    private async Task RestartProjectionsThatShouldBeRunningAsync(CancellationToken cancellationToken)
    {
        var daemon = _daemonAccessor.Daemon;
        if (daemon is null)
        {
            return;
        }

        // Anything not running is a candidate, not just what the daemon calls paused: a shard can also end up merely
        // stopped - a start that failed, a stop nobody meant to keep - and that is just as much "not doing what it is
        // supposed to". Filtering here rather than on HasAnyPaused also keeps the healthy case free of any I/O.
        var notRunning = _martenProjections
            .Where(projection => daemon.StatusFor(projection.Id) != AgentStatus.Running)
            .ToList();
        if (notRunning.Count == 0)
        {
            return;
        }

        var desiredStates = await _desiredStateStore.GetAllAsync(cancellationToken);

        foreach (var projection in notRunning)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var desiredState = desiredStates.GetValueOrDefault(projection.Id) ?? projection.FallbackDesiredState;
            if (!string.Equals(desiredState, ProjectionDesiredStates.Subscribed, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("{ShardName} is not running, but its desired state is {DesiredState}; leaving it alone.",
                    projection.Id, desiredState);
                continue;
            }

            // Logged at Error level so a projection that keeps falling over surfaces on Slack (the Slack sink only
            // forwards Error and above).
            _logger.LogError("{ShardName} is {ActualState} while it is supposed to be running; attempting to restart it.",
                projection.Id, ProjectionHealth.DescribeAgentStatus(daemon.StatusFor(projection.Id)));

            try
            {
                await daemon.StartAgentAsync(projection.Id, cancellationToken);
                _logger.LogInformation("Requested a restart of {ShardName}.", projection.Id);
            }
            catch (Exception ex)
            {
                // One shard failing to restart must not stop the others from being tried.
                _logger.LogWarning(ex, "Could not restart {ShardName}; will try again next interval.", projection.Id);
            }
        }
    }
}
