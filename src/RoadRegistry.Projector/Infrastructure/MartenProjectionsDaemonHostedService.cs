namespace RoadRegistry.Projector.Infrastructure;

using System;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.Events.Daemon;
using Marten;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Starts the Marten async projection daemon. Runs as a hosted service (so it participates in the host lifecycle: the
// store is injected up front - never resolved from a possibly-disposed provider after an unbounded async gap - a
// shutdown during startup cancels cleanly, and the daemon is stopped when the host stops). The schema migrations have
// already been applied by the IDbMigrators run in Program before the host started, so the tables exist by now.
public sealed class MartenProjectionsDaemonHostedService : IHostedService
{
    private readonly IDocumentStore _store;
    private readonly MartenProjectionDaemonAccessor _daemonAccessor;
    private readonly ILogger<IProjectionDaemon> _logger;
    private IProjectionDaemon? _daemon;

    public MartenProjectionsDaemonHostedService(
        IDocumentStore store,
        MartenProjectionDaemonAccessor daemonAccessor,
        ILogger<IProjectionDaemon> logger)
    {
        _store = store;
        _daemonAccessor = daemonAccessor;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IProjectionDaemon? daemon = null;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            daemon = await _store.BuildProjectionDaemonAsync(logger: _logger);
            await daemon.StartAllAsync();

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
