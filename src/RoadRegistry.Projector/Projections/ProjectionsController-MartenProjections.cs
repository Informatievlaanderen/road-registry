namespace RoadRegistry.Projector.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
using Infrastructure;
using JasperFx;
using JasperFx.Events.Daemon;
using Marten;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Pbs.Projections;
using RoadRegistry.Pbs.Schema;
using RoadRegistry.Pbs.Schema.Records;
using RoadRegistry.Read.Projections.Setup;
using RoadRegistry.WmsWfsV1Inwinning;
using RoadRegistry.WmsWfsV1Inwinning.Projections;
using RoadRegistry.WmsWfsV2.Projections;
using RoadRegistry.WmsWfsV2.Schema;

public partial class ProjectionsController
{
    /// <summary>
    /// Stops a Marten projection so it no longer processes events.
    /// </summary>
    /// <remarks>
    /// Waits until the daemon reports the shard as fully stopped before answering, and records that the projection is
    /// meant to stay stopped, so neither the supervisor nor the next restart of the host brings it back.
    ///
    /// Marten stops a shard by draining it - it finishes every page it had already fetched - and that drain is
    /// unbounded, so on a projection that is behind it can take hours. The shard is therefore given drainSeconds to
    /// stop on its own, after which the batch it is working on is cancelled. That batch was never committed, so it
    /// replays from the projection's last committed position when the projection is started again.
    /// </remarks>
    /// <param name="id">The projection id, case-insensitive, with or without the ":All" shard suffix (e.g. "RoadNetworkChangesExtractProjection").</param>
    /// <param name="daemonAccessor"></param>
    /// <param name="shardStopper"></param>
    /// <param name="drainSeconds">How long the projection may take to finish the work it already has in hand before
    /// the batch it is working on is cancelled. Zero cancels it right away.</param>
    /// <param name="cancellationToken"></param>
    [HttpGet("{id}/stop")]
    [HttpPost("{id}/stop")]
    public async Task<IActionResult> StopMartenProjection(
        [FromRoute] string id,
        [FromServices] MartenProjectionDaemonAccessor daemonAccessor,
        [FromServices] MartenShardStopper shardStopper,
        [FromQuery] int drainSeconds = 30,
        CancellationToken cancellationToken = default)
    {
        var projection = FindMartenProjection(id);
        if (projection is null)
        {
            return NotFound($"Projection {id} is not known or not enabled on this host.");
        }

        var daemon = daemonAccessor.Daemon;
        if (daemon is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "The Marten projection daemon is not running.");
        }

        var shardName = projection.Id;

        // Record the intent first and unconditionally. A paused shard is not stopped - it is a shard that fell over and
        // that the supervisor would otherwise resume - so "it is already down" is not a reason to skip saying that it
        // should stay down.
        await _projectionStateStore.SetDesiredStateAsync(shardName, ProjectionDesiredStates.Stopped, cancellationToken);

        var status = daemon.StatusFor(shardName);
        if (status != AgentStatus.Running)
        {
            return Ok($"{shardName} was already {DescribeStatus(status)}; its desired state is now stopped.");
        }

        var drainGracePeriod = TimeSpan.FromSeconds(Math.Clamp(drainSeconds, 0, (int)TimeSpan.FromHours(1).TotalSeconds));
        var outcome = await shardStopper.StopAsync(daemon, shardName, drainGracePeriod, cancellationToken);

        return outcome switch
        {
            MartenShardStopOutcome.Drained => Ok($"{shardName} stopped."),
            MartenShardStopOutcome.BatchCancelled => Ok(
                $"{shardName} stopped. It did not stop on its own within {drainGracePeriod.TotalSeconds}s, so the batch it was working on was cancelled; that batch was never committed and replays from the projection's last committed position when it is started again."),
            _ => StatusCode(StatusCodes.Status504GatewayTimeout,
                $"{shardName} is still running, even after the batch it was working on was cancelled. Its desired state is stopped, so it will not be started again; check the logs.")
        };
    }

    /// <summary>
    /// Starts a stopped Marten projection so it resumes processing events from its last position.
    /// </summary>
    /// <param name="id">The projection id, case-insensitive, with or without the ":All" shard suffix (e.g. "RoadNetworkChangesExtractProjection").</param>
    /// <param name="daemonAccessor"></param>
    /// <param name="cancellationToken"></param>
    [HttpGet("{id}/start")]
    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartMartenProjection(
        [FromRoute] string id,
        [FromServices] MartenProjectionDaemonAccessor daemonAccessor,
        CancellationToken cancellationToken)
    {
        var projection = FindMartenProjection(id);
        if (projection is null)
        {
            return NotFound($"Projection {id} is not known or not enabled on this host.");
        }

        var daemon = daemonAccessor.Daemon;
        if (daemon is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "The Marten projection daemon is not running.");
        }

        var shardName = projection.Id;

        // The backfill only notices a started projection between two chunks; one started under it would race the
        // chunk it is writing.
        if (shardName == WellKnownProjectionStateNames.RoadNetworkChangesPbsProjection && Volatile.Read(ref _pbsBackfillRunning) != 0)
        {
            return Conflict($"{shardName} cannot be started while the backfill of the afgeleide schijnknopen is running; wait until the log says it completed.");
        }

        // Before starting, so that a start which then fails still leaves the supervisor with a mandate to retry.
        await _projectionStateStore.SetDesiredStateAsync(shardName, ProjectionDesiredStates.Subscribed, cancellationToken);

        if (daemon.StatusFor(shardName) == AgentStatus.Running)
        {
            return Ok($"{shardName} is already running.");
        }

        await daemon.StartAgentAsync(shardName, cancellationToken);

        return Ok($"{shardName} started.");
    }

    /// <summary>
    /// Rebuilds a projection: wipes its read model and replays the full event stream.
    /// </summary>
    /// <remarks>
    /// The projection has to be stopped first (via {id}/stop); a running projection is refused with a 409 so the
    /// truncate never races a batch that is still being processed. The read model and every progression the
    /// projection keeps are wiped, then the projection is started again and replays the event stream in the
    /// background; the call returns as soon as the replay has started.
    /// </remarks>
    /// <param name="id">The projection id, case-insensitive, with or without the ":All" shard suffix (e.g. "RoadNetworkChangesExtractProjection").</param>
    /// <param name="daemonAccessor"></param>
    /// <param name="cancellationToken"></param>
    [HttpGet("{id}/rebuild")]
    [HttpPost("{id}/rebuild")]
    public async Task<IActionResult> RebuildMartenProjection(
        [FromRoute] string id,
        [FromServices] MartenProjectionDaemonAccessor daemonAccessor,
        CancellationToken cancellationToken = default)
    {
        var projection = FindMartenProjection(id);
        if (projection is null)
        {
            return NotFound($"Projection {id} is not known or not enabled on this host.");
        }

        var truncateReadModel = GetTruncateReadModel(projection.Id);
        if (truncateReadModel is null)
        {
            return BadRequest($"Projection {projection.Id} has no rebuild support.");
        }

        var daemon = daemonAccessor.Daemon;
        if (daemon is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "The Marten projection daemon is not running.");
        }

        // Wipes the read model and every progression the projection keeps - its per-correlation progression
        // documents and Marten's own shard progression - and starts the shard, so it replays the full event
        // stream in the background. Returns as soon as the replay has started. The shard has to be stopped
        // explicitly first: truncating while it still processes a batch would race the projection against its
        // own teardown, so this never stops it on the caller's behalf.
        var shardName = projection.Id;
        var projectionName = shardName[..shardName.IndexOf(':')];

        if (shardName == WellKnownProjectionStateNames.RoadNetworkChangesPbsProjection && Volatile.Read(ref _pbsBackfillRunning) != 0)
        {
            return Conflict($"{shardName} cannot be rebuilt while the backfill of the afgeleide schijnknopen is running; wait until the log says it completed.");
        }

        if (daemon.StatusFor(shardName) == AgentStatus.Running)
        {
            return Conflict($"{shardName} is still running; stop it via projections/{id}/stop before rebuilding.");
        }

        _logger.LogWarning("Rebuilding {ShardName}: truncating the read model and resetting the progressions.", shardName);

        await truncateReadModel(cancellationToken);

        await using (var session = _documentStore.LightweightSession())
        {
            session.DeleteWhere<RoadNetworkChangesProjectionProgression>(x => x.ProjectionName == projectionName);
            // Marten's shard progression; without this the restarted agent resumes at the old high water mark
            // instead of replaying from the start.
            session.QueueSqlCommand($"DELETE FROM {WellKnownSchemas.MartenEventStore}.mt_event_progression WHERE name = ?;", shardName);
            await session.SaveChangesAsync(cancellationToken);
        }

        await _projectionStateStore.SetDesiredStateAsync(shardName, ProjectionDesiredStates.Subscribed, cancellationToken);
        await daemon.StartAgentAsync(shardName, cancellationToken);
        _logger.LogWarning("Rebuilding {ShardName}: the shard was started and replays from the beginning of the event stream.", shardName);

        return Ok($"{shardName} is rebuilding: the read model was truncated and the projection replays from the start of the event stream.");
    }

    // The daemon has three states and they are not interchangeable: a paused shard fell over on its own, a stopped one
    // was told to stop. Saying which one it is, is the difference between "nothing to do" and "something is wrong".
    private static string DescribeStatus(AgentStatus status)
    {
        return status switch
        {
            AgentStatus.Running => "running",
            AgentStatus.Paused => "paused after an error",
            AgentStatus.Stopped => "stopped",
            _ => status.ToString().ToLowerInvariant()
        };
    }

    // Case-insensitive on the projection id, with or without the ":All" shard suffix, so
    // "roadnetworkchangesextractprojection" resolves to "RoadNetworkChangesExtractProjection:All".
    private ProjectionDetail? FindMartenProjection(string id)
    {
        return _martenProjections.FirstOrDefault(x =>
            string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase)
            || string.Equals(x.Id, $"{id}:All", StringComparison.OrdinalIgnoreCase));
    }

    private Func<CancellationToken, Task>? GetTruncateReadModel(string shardName)
    {
        return shardName switch
        {
            WellKnownProjectionStateNames.RoadNetworkChangesExtractProjection => TruncateExtractReadModel,
            WellKnownProjectionStateNames.RoadNetworkChangesReadProjection => TruncateReadReadModel,
            WellKnownProjectionStateNames.RoadNetworkChangesPbsProjection => TruncatePbsReadModel,
            WellKnownProjectionStateNames.RoadNetworkChangesWmsWfsV2Projection => TruncateWmsWfsV2ReadModel,
            WellKnownProjectionStateNames.RoadNetworkChangesWmsWfsV1InwinningProjection => TruncateWmsWfsV1InwinningReadModel,
            WellKnownProjectionStateNames.RoadNetworkChangesPbsTempProjection => TruncatePbsTempReadModel,
            WellKnownProjectionStateNames.RoadNetworkChangesWmsWfsV2TempProjection => TruncateWmsWfsV2TempReadModel,
            _ => null
        };
    }

    private Task TruncateExtractReadModel(CancellationToken cancellationToken)
    {
        return DeleteDocuments(MartenProjectionDocuments.GetDocumentTypes(options => options.ConfigureExtractDocuments()), cancellationToken);
    }

    private Task TruncateReadReadModel(CancellationToken cancellationToken)
    {
        return DeleteDocuments(MartenProjectionDocuments.GetDocumentTypes(options => options.ConfigureReadDocuments()), cancellationToken);
    }

    private async Task TruncatePbsReadModel(CancellationToken cancellationToken)
    {
        var factory = HttpContext.RequestServices.GetRequiredService<IDbContextFactory<PbsContext>>();
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        // The enum-based code lists are synced by PbsCodeListSyncService instead of by events, so a replay cannot
        // restore them; everything else in the model is projection output and goes.
        await TruncateProjectionTables(context, nameof(RoadNetworkChangesPbsProjection),
            clrType => typeof(IEnumBasedCodeListRecord).IsAssignableFrom(clrType), cancellationToken);
    }

    // The shadow read models, truncated through a context scoped to their own schema. Same tables, same rules - a
    // rebuild of a rebuild, which is what a shadow that went wrong needs.
    private async Task TruncatePbsTempReadModel(CancellationToken cancellationToken)
    {
        var factory = HttpContext.RequestServices.GetRequiredService<TempSchemaDbContextFactory<PbsContext>>();
        await using var context = factory.CreateDbContext();

        await TruncateProjectionTables(context, nameof(RoadNetworkChangesPbsTempProjection),
            clrType => typeof(IEnumBasedCodeListRecord).IsAssignableFrom(clrType), cancellationToken);
    }

    private async Task TruncateWmsWfsV2TempReadModel(CancellationToken cancellationToken)
    {
        var factory = HttpContext.RequestServices.GetRequiredService<TempSchemaDbContextFactory<WmsWfsV2Context>>();
        await using var context = factory.CreateDbContext();

        await TruncateProjectionTables(context, nameof(RoadNetworkChangesWmsWfsV2TempProjection), excludeEntity: null, cancellationToken);
    }

    private async Task TruncateWmsWfsV2ReadModel(CancellationToken cancellationToken)
    {
        var factory = HttpContext.RequestServices.GetRequiredService<IDbContextFactory<WmsWfsV2Context>>();
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        await TruncateProjectionTables(context, nameof(RoadNetworkChangesWmsWfsV2Projection), excludeEntity: null, cancellationToken);
    }

    private async Task TruncateWmsWfsV1InwinningReadModel(CancellationToken cancellationToken)
    {
        var factory = HttpContext.RequestServices.GetRequiredService<IDbContextFactory<WmsWfsV1InwinningContext>>();
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        // Only the lists: the V1 tables are mapped, not migrated, so they are left alone - a replay sets their IsV2
        // flags again.
        await TruncateProjectionTables(context, nameof(RoadNetworkChangesWmsWfsV1InwinningProjection), excludeEntity: null, cancellationToken);
    }

    // Every table in the model - so a newly added table is wiped automatically - except the projection-state row
    // (deleted by name, it is the SQL-side idempotency position that would otherwise make the restarted projection skip
    // every replayed event), a table the context maps but does not migrate - it belongs to another read model - and
    // whatever the caller excludes.
    internal static IReadOnlyList<string> GetTablesToTruncate(DbContext context, Func<Type, bool>? excludeEntity)
    {
        // The design-time model: the read-optimized runtime model does not keep which tables are excluded from migrations.
        var tables = new List<string>();
        foreach (var entityType in context.GetService<IDesignTimeModel>().Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (clrType == typeof(ProjectionStateItem) || entityType.IsTableExcludedFromMigrations() || excludeEntity?.Invoke(clrType) == true)
            {
                continue;
            }

            var table = entityType.GetTableName();
            if (table is null)
            {
                continue;
            }

            var schema = entityType.GetSchema();
            tables.Add(schema is null ? $"[{table}]" : $"[{schema}].[{table}]");
        }

        return tables;
    }

    private async Task DeleteDocuments(Type[] documentTypes, CancellationToken cancellationToken)
    {
        foreach (var documentType in documentTypes)
        {
            await _documentStore.Advanced.Clean.DeleteDocumentsByTypeAsync(documentType, cancellationToken);
        }
    }

    private async Task TruncateProjectionTables<TContext>(
        TContext context,
        string projectionName,
        Func<Type, bool>? excludeEntity,
        CancellationToken cancellationToken)
        where TContext : Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.RunnerDbContext<TContext>
    {
        var tables = GetTablesToTruncate(context, excludeEntity);

        // All of it in one transaction. TRUNCATE and view DDL are both transactional in SQL Server, so a process that
        // dies between dropping a view and recreating it leaves the read model exactly as it was - views included -
        // rather than without the views that serve it. The views are the read model's only copy of their own
        // definition once dropped, which is what makes that worth insisting on.
        //
        // Through the execution strategy, because the connection is configured to retry and a retrying strategy
        // refuses a transaction it did not open itself: an attempt has to replay the whole unit rather than resume
        // inside it. That is also exactly what is wanted here - a transient failure rolls the transaction back, views
        // and all, and the next attempt captures them again from scratch.
        // The token goes to the strategy as well as into the work, so a cancelled request also ends the wait between
        // two attempts. Without it the retry loop backs off on CancellationToken.None and then starts another attempt
        // on a request that is already gone.
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async token =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(token);

            // A view WITH SCHEMABINDING blocks TRUNCATE on everything it is bound to, so the serving views come off
            // first and go back on at the end - recreated from the definition the server itself just handed us, never
            // from a copy in this repository that could have gone stale. See SqlServerViewMaintenance.
            var views = await SqlServerViewMaintenance.CaptureAsync(context, tables, token);
            await SqlServerViewMaintenance.DropAsync(context, views, _logger, token);

            foreach (var qualifiedName in tables)
            {
                _logger.LogInformation("Rebuilding {ProjectionName}: truncating table {Table}.", projectionName, qualifiedName);
                await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {qualifiedName};", token);
            }

            await SqlServerViewMaintenance.RecreateAsync(context, views, _logger, token);

            await context.ProjectionStates
                .Where(x => x.Name == projectionName)
                .ExecuteDeleteAsync(token);

            await transaction.CommitAsync(token);
        }, cancellationToken);
    }
}
