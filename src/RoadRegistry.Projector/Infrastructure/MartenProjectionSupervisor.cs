namespace RoadRegistry.Projector.Infrastructure;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JasperFx;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;

// Keeps the Marten async projections alive without letting a single projection's failures affect the rest of the host.
// When a projection throws, its (isolated) Marten shard is paused while every other shard keeps running. This supervisor
// periodically restarts any shard that is not running while it is supposed to be, so it recovers on its own once the
// underlying problem clears. All errors are swallowed and logged: it must never bring the host down.
//
// Two things it deliberately does not do. It does not touch a projection whose desired state is "stopped" - it used to
// call StartAllAsync, which resumed everything that was not running and so undid a deliberate stop within the restart
// interval, including during maintenance on that very projection. And it does not retry a failing projection at full
// speed forever: see ProjectionRestartPolicy.
public sealed class MartenProjectionSupervisor : BackgroundService
{
    private readonly MartenProjectionDaemonAccessor _daemonAccessor;
    private readonly IReadOnlyList<ProjectionDetail> _martenProjections;
    private readonly MartenProjectionStateStore _projectionStateStore;
    private readonly ProjectionRestartPolicy _restartPolicy;
    private readonly TimeSpan _interval;
    private readonly ILogger<MartenProjectionSupervisor> _logger;

    public MartenProjectionSupervisor(
        MartenProjectionDaemonAccessor daemonAccessor,
        IReadOnlyList<ProjectionDetail> martenProjections,
        MartenProjectionStateStore projectionStateStore,
        IConfiguration configuration,
        ILogger<MartenProjectionSupervisor> logger)
    {
        _daemonAccessor = daemonAccessor;
        _martenProjections = martenProjections;
        _projectionStateStore = projectionStateStore;
        _restartPolicy = new ProjectionRestartPolicy();
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
        // stopped - a start that failed, a host that came up while the database was down - and that is just as much
        // "not doing what it is supposed to". Every shard that is running clears its restart count here, so recovery
        // hands the fast attempts back.
        var notRunning = new List<ProjectionDetail>();
        foreach (var projection in _martenProjections)
        {
            if (daemon.StatusFor(projection.Id) == AgentStatus.Running)
            {
                _restartPolicy.RecordRunning(projection.Id);
            }
            else
            {
                notRunning.Add(projection);
            }
        }

        if (notRunning.Count == 0)
        {
            return;
        }

        // Only once something is actually down, so the healthy case stays free of I/O.
        var desiredStates = await _projectionStateStore.GetDesiredStatesAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        foreach (var projection in notRunning)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var desiredState = desiredStates.DesiredStateOf(projection);
            if (!desiredState.ShouldBeRunning())
            {
                _logger.LogDebug("{ShardName} is not running, but its desired state is {DesiredState}; leaving it alone.",
                    projection.Id, desiredState);
                continue;
            }

            if (!_restartPolicy.ShouldAttemptRestart(projection.Id, now))
            {
                _logger.LogDebug("{ShardName} has failed to stay up {Attempts} times; waiting for the slow retry.",
                    projection.Id, _restartPolicy.AttemptsFor(projection.Id));
                continue;
            }

            var backingOff = _restartPolicy.IsBackingOff(projection.Id);
            _restartPolicy.RecordAttempt(projection.Id, now);

            // Logged at Error level so a projection that keeps falling over surfaces on Slack (the Slack sink only
            // forwards Error and above). Once the policy is backing off this happens hourly rather than every tick.
            _logger.LogError("{ShardName} is {ActualState} while it is supposed to be running; restarting it (attempt {Attempt}{Backoff}).",
                projection.Id,
                DescribeAgentStatus(daemon.StatusFor(projection.Id)),
                _restartPolicy.AttemptsFor(projection.Id),
                backingOff ? ", slow retry" : string.Empty);

            try
            {
                await daemon.StartAgentAsync(projection.Id, cancellationToken);
                _logger.LogInformation("Requested a restart of {ShardName}.", projection.Id);
            }
            catch (Exception ex)
            {
                // One shard failing to restart must not stop the others from being tried.
                _logger.LogWarning(ex, "Could not restart {ShardName}; will try again later.", projection.Id);
            }
        }
    }

    // The daemon has three states and they are not interchangeable: a shard that fell over is paused, one that was told
    // to stop is stopped. Saying which one it is, is the difference between "nothing to do" and "something is wrong".
    private static string DescribeAgentStatus(AgentStatus status)
    {
        return status switch
        {
            AgentStatus.Running => "running",
            AgentStatus.Paused => "paused after an error",
            AgentStatus.Stopped => "stopped",
            _ => status.ToString().ToLowerInvariant()
        };
    }
}
