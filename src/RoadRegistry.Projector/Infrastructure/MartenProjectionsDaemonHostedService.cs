namespace RoadRegistry.Projector.Infrastructure;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.Events.Daemon;
using Marten;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;

// Starts the Marten async projection daemon. Runs as a hosted service (so it participates in the host lifecycle: the
// store is injected up front - never resolved from a possibly-disposed provider after an unbounded async gap - a
// shutdown during startup cancels cleanly, and the daemon is stopped when the host stops). The schema migrations have
// already been applied by the IDbMigrators run in Program before the host started, so the tables exist by now.
//
// Only the shards that are supposed to be running are started: a projection an operator stopped stays stopped across a
// deploy or a restart, which is what StartAllAsync used to undo. Starting a shard individually also starts the high
// water detection, so nothing else is needed to get the daemon going.
public sealed class MartenProjectionsDaemonHostedService : IHostedService
{
    private readonly IDocumentStore _store;
    private readonly MartenProjectionDaemonAccessor _daemonAccessor;
    private readonly IReadOnlyList<ProjectionDetail> _martenProjections;
    private readonly MartenProjectionStateStore _projectionStateStore;
    private readonly ILogger<IProjectionDaemon> _logger;
    private IProjectionDaemon? _daemon;

    public MartenProjectionsDaemonHostedService(
        IDocumentStore store,
        MartenProjectionDaemonAccessor daemonAccessor,
        IReadOnlyList<ProjectionDetail> martenProjections,
        MartenProjectionStateStore projectionStateStore,
        ILogger<IProjectionDaemon> logger)
    {
        _store = store;
        _daemonAccessor = daemonAccessor;
        _martenProjections = martenProjections;
        _projectionStateStore = projectionStateStore;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IProjectionDaemon? daemon = null;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            daemon = await _store.BuildProjectionDaemonAsync(logger: _logger);
            await StartSubscribedShardsAsync(daemon, cancellationToken);

            _daemon = daemon;
            _daemonAccessor.Daemon = daemon;
            daemon = null;
        }
        catch (OperationCanceledException)
        {
            // Host is shutting down before the migrations completed / daemon started; there is nothing to start.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Marten projection daemon failed to start");
        }
        finally
        {
            daemon?.Dispose();
        }
    }

    private async Task StartSubscribedShardsAsync(IProjectionDaemon daemon, CancellationToken cancellationToken)
    {
        Dictionary<string, string> desiredStates;
        try
        {
            desiredStates = await _projectionStateStore.GetDesiredStatesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Not being able to read the intent must not leave the host running with every projection down - that is
            // the failure this whole mechanism exists to make visible, and it would be invisible here. Fall back to
            // what the daemon did before there was a desired state: start everything.
            _logger.LogError(ex, "Could not read the desired state of the Marten projections; starting all of them.");
            desiredStates = [];
        }

        foreach (var projection in _martenProjections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var desiredState = desiredStates.DesiredStateOf(projection);
            if (!desiredState.ShouldBeRunning())
            {
                _logger.LogWarning("Not starting {ShardName}: its desired state is {DesiredState}.", projection.Id, desiredState);
                continue;
            }

            // One shard failing to start must not keep the others down; the supervisor retries this one.
            try
            {
                await daemon.StartAgentAsync(projection.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not start {ShardName}.", projection.Id);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var daemon = Interlocked.Exchange(ref _daemon, null);
        _daemonAccessor.Daemon = null;
        if (daemon is null)
        {
            return;
        }

        try
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                await daemon.StopAllAsync();
            }
        }
        finally
        {
            daemon.Dispose();
        }
    }
}
