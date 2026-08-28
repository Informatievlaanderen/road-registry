namespace RoadRegistry.Projector.Infrastructure;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JasperFx;
using JasperFx.Events.Daemon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Keeps the Marten async projections alive without letting a single projection's failures affect the rest of the host.
// When a projection throws, its (isolated) Marten shard is paused while every other shard keeps running. This supervisor
// periodically resumes a paused shard so it recovers on its own once the underlying problem clears. All errors are
// swallowed and logged - it must never bring the host down.
//
// It only resumes shards whose desired state says they should be running. A projection an operator stopped is left
// alone: previously this used StartAllAsync, which resumed every shard that was not running and so undid a deliberate
// stop within the restart interval - including during maintenance on that very projection.
public sealed class MartenProjectionSupervisor : BackgroundService
{
    private readonly MartenProjectionDaemonAccessor _daemonAccessor;
    private readonly IReadOnlyList<ProjectionDetail> _martenProjections;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval;
    private readonly ILogger<MartenProjectionSupervisor> _logger;

    public MartenProjectionSupervisor(
        MartenProjectionDaemonAccessor daemonAccessor,
        IReadOnlyList<ProjectionDetail> martenProjections,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<MartenProjectionSupervisor> logger)
    {
        _daemonAccessor = daemonAccessor;
        _martenProjections = martenProjections;
        _serviceProvider = serviceProvider;
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

        var desiredStates = await _serviceProvider
            .GetRequiredService<MartenProjectionDesiredStateStore>()
            .GetAllAsync(cancellationToken);

        foreach (var projection in _martenProjections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (daemon.StatusFor(projection.Id) == AgentStatus.Running)
            {
                continue;
            }

            var desiredState = desiredStates.GetValueOrDefault(projection.Id) ?? projection.FallbackDesiredState;
            if (!string.Equals(desiredState, ProjectionDesiredStates.Subscribed, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("{ShardName} is not running, but its desired state is {DesiredState}; leaving it alone.",
                    projection.Id, desiredState);
                continue;
            }

            // Logged at Error level so a projection that keeps falling over surfaces on Slack (the Slack sink only
            // forwards Error and above).
            _logger.LogError("{ShardName} is not running while it is supposed to be; attempting to restart it.", projection.Id);

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
