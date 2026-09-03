namespace RoadRegistry.Projector.Projections;

using System;
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
using RoadRegistry.WmsWfsV2.Projections;
using RoadRegistry.WmsWfsV2.Schema;

public partial class ProjectionsController
{
    private const string TopologyProjectionId = "topology";

    /// <summary>
    /// Stops a Marten projection so it no longer processes events.
    /// </summary>
    /// <remarks>
    /// Waits until the daemon reports the shard as fully stopped before answering, and records that the projection is
    /// meant to stay stopped, so neither the supervisor nor the next restart of the host brings it back. The special
    /// id "topology" is not supported here: the topology projection runs inline with the event store writes and cannot
    /// be stopped.
    /// </remarks>
    /// <param name="id">The projection id, case-insensitive, with or without the ":All" shard suffix (e.g. "RoadNetworkChangesExtractProjection").</param>
    /// <param name="daemonAccessor"></param>
    /// <param name="cancellationToken"></param>
    [HttpGet("{id}/stop")]
    [HttpPost("{id}/stop")]
    public async Task<IActionResult> StopMartenProjection(
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

        // Record the intent first and unconditionally. A paused shard is not stopped - it is a shard that fell over and
        // that the supervisor would otherwise resume - so "it is already down" is not a reason to skip saying that it
        // should stay down.
        await _projectionStateStore.SetDesiredStateAsync(shardName, ProjectionDesiredStates.Stopped, cancellationToken);

        var status = daemon.StatusFor(shardName);
        if (status != AgentStatus.Running)
        {
            return Ok($"{shardName} was already {DescribeStatus(status)}; its desired state is now stopped.");
        }

        await daemon.StopAgentAsync(shardName, null);
        await WaitUntilShardStopped(daemon, shardName, cancellationToken);

        return Ok($"{shardName} stopped.");
    }

    /// <summary>
    /// Starts a stopped Marten projection so it resumes processing events from its last position.
    /// </summary>
    /// <remarks>
    /// The special id "topology" is not supported here: the topology projection runs inline with the event store
    /// writes and is always running.
    /// </remarks>
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
    ///
    /// Special rule for the id "topology": the topology projection runs inline and is rebuilt through a one-off
    /// daemon instead - it does not need to be stopped first, and the call blocks until the rebuild completed
    /// (bounded by timeoutHours).
    /// </remarks>
    /// <param name="id">The projection id, case-insensitive, with or without the ":All" shard suffix (e.g. "RoadNetworkChangesExtractProjection"), or "topology".</param>
    /// <param name="daemonAccessor"></param>
    /// <param name="configuration"></param>
    /// <param name="timeoutHours">Only used for "topology": how long the blocking rebuild may take.</param>
    /// <param name="cancellationToken"></param>
    [HttpGet("{id}/rebuild")]
    [HttpPost("{id}/rebuild")]
    public async Task<IActionResult> RebuildMartenProjection(
        [FromRoute] string id,
        [FromServices] MartenProjectionDaemonAccessor daemonAccessor,
        [FromServices] IConfiguration configuration,
        [FromQuery] int timeoutHours = 12,
        CancellationToken cancellationToken = default)
    {
        if (IsTopology(id))
        {
            return await RebuildTopology(configuration, timeoutHours, cancellationToken);
        }

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

    // The topology projection runs inline, so its rebuild goes through a one-off daemon built for just this
    // projection: Marten tears the topology tables down (DeleteDataInTableOnTeardown) and replays the full event
    // stream through the current handlers. Blocks until the rebuild completed.
    private async Task<IActionResult> RebuildTopology(IConfiguration configuration, int timeoutHours, CancellationToken cancellationToken)
    {
        var sp = new ServiceCollection()
            .AddSingleton(configuration)
            .AddMartenRoad(options =>
            {
                options.AddRoadNetworkTopologyProjection();
            }).Services
            .BuildServiceProvider();

        var store = sp.GetRequiredService<IDocumentStore>();
        var projectionDaemon = await store.BuildProjectionDaemonAsync();
        await projectionDaemon.RebuildProjectionAsync<RoadNetworkTopologyProjection>(TimeSpan.FromHours(timeoutHours), cancellationToken);

        return Ok($"{nameof(RoadNetworkTopologyProjection)} rebuild completed.");
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

    private static bool IsTopology(string id)
    {
        return string.Equals(id, TopologyProjectionId, StringComparison.OrdinalIgnoreCase);
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

    private async Task TruncateWmsWfsV2ReadModel(CancellationToken cancellationToken)
    {
        var factory = HttpContext.RequestServices.GetRequiredService<IDbContextFactory<WmsWfsV2Context>>();
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        await TruncateProjectionTables(context, nameof(RoadNetworkChangesWmsWfsV2Projection), excludeEntity: null, cancellationToken);
    }

    private static async Task WaitUntilShardStopped(IProjectionDaemon daemon, string shardName, CancellationToken cancellationToken)
    {
        var timeout = TimeSpan.FromMinutes(1);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (daemon.StatusFor(shardName) == AgentStatus.Running)
        {
            if (stopwatch.Elapsed > timeout)
            {
                throw new InvalidOperationException($"Shard {shardName} was still running {timeout.TotalSeconds}s after it was told to stop; not touching the read model.");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
        }
    }

    private async Task DeleteDocuments(Type[] documentTypes, CancellationToken cancellationToken)
    {
        foreach (var documentType in documentTypes)
        {
            await _documentStore.Advanced.Clean.DeleteDocumentsByTypeAsync(documentType, cancellationToken);
        }
    }

    // Truncates every table in the model - so a newly added table is wiped automatically - except the
    // projection-state row (deleted by name, it is the SQL-side idempotency position that would otherwise make
    // the restarted projection skip every replayed event) and whatever the caller excludes.
    private async Task TruncateProjectionTables<TContext>(
        TContext context,
        string projectionName,
        Func<Type, bool>? excludeEntity,
        CancellationToken cancellationToken)
        where TContext : Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.RunnerDbContext<TContext>
    {
        foreach (var entityType in context.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (clrType == typeof(ProjectionStateItem) || excludeEntity?.Invoke(clrType) == true)
            {
                continue;
            }

            var table = entityType.GetTableName();
            if (table is null)
            {
                continue;
            }

            var schema = entityType.GetSchema();
            var qualifiedName = schema is null ? $"[{table}]" : $"[{schema}].[{table}]";
            _logger.LogInformation("Rebuilding {ProjectionName}: truncating table {Table}.", projectionName, qualifiedName);
            await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {qualifiedName};", cancellationToken);
        }

        await context.ProjectionStates
            .Where(x => x.Name == projectionName)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
