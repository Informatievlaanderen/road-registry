namespace RoadRegistry.Projector.Infrastructure;

using System;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.Events.Daemon;
using Marten;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Setup;

// Starts the Marten async projection daemon once the schema migrations have been applied. Runs as a hosted service
// (registered after DatabaseMigrator, so the migrations gate is already completed by the time it starts) instead of
// being fire-and-forgotten from Startup.Configure. This way it participates in the host lifecycle: the store and gate
// are injected up front (never resolved from a possibly-disposed provider after an unbounded async gap), a shutdown
// during startup cancels cleanly, and the daemon is stopped when the host stops.
public sealed class MartenProjectionsDaemonHostedService : IHostedService
{
    private readonly IDocumentStore _store;
    private readonly DatabaseMigrationsGate _migrationsGate;
    private readonly ILogger<IProjectionDaemon> _logger;
    private IProjectionDaemon? _daemon;

    public MartenProjectionsDaemonHostedService(
        IDocumentStore store,
        DatabaseMigrationsGate migrationsGate,
        ILogger<IProjectionDaemon> logger)
    {
        _store = store;
        _migrationsGate = migrationsGate;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IProjectionDaemon? daemon = null;

        try
        {
            // Wait for DbUp to finish applying the schema migrations before starting the daemon; otherwise, on a
            // fresh database, the daemon would fail against not-yet-created tables and never retry.
            await _migrationsGate.Completed.WaitAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            daemon = await _store.BuildProjectionDaemonAsync(logger: _logger);
            await daemon.StartAllAsync();

            _daemon = daemon;
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
