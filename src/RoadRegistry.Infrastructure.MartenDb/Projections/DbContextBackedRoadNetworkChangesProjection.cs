namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using System;
using System.Diagnostics;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// Drives a set of RunnerDbContext-backed sub-projections against a single SQL Server read model. For each daemon batch
// it creates one TDbContext, applies every sub-projection's handlers for every event, and commits once - advancing a
// single projection-state row (keyed by this projection's name) atomically with the read-model writes. That single
// position is the SQL-side idempotency guard for events re-delivered after the Marten progression and the SQL Server
// write diverged; it replaces the per-sub-projection positions that used to exist. Because a batch here is exactly the
// page it was given, that one position covers every event the batch applied, so a re-delivered page is skipped whole
// and the handlers do not have to be idempotent themselves.
//
// Two things speed up the write side, both under ProjectionCatchUpOptions, neither changing what is written:
//   - a batch's high-volume inserts are streamed with SqlBulkCopy instead of EF's per-row INSERT batching. This is
//     driven by how many rows of one type a batch holds, not by catching up, so a large live batch benefits too;
//   - the read model's non-clustered indexes are disabled while catching up on a backlog big enough to be worth it,
//     and rebuilt once the tail is reached.
// The commit stays atomic: when a batch bulk-copies, the copy runs on the context's own connection inside an
// explicit transaction that SaveChangesAsync then commits, so the rows and the position still land together or not at
// all. A batch without a bulk copy needs no transaction of its own - SaveChangesAsync is atomic already.
public abstract class DbContextBackedRoadNetworkChangesProjection<TDbContext> : RoadNetworkChangesProjection
    where TDbContext : RunnerDbContext<TDbContext>
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly IReadOnlyCollection<IRoadNetworkChangesProjection<TDbContext>> _projections;
    private readonly ProjectionCatchUpOptions _options;
    private readonly ProjectionThroughputMetrics _metrics;

    // Turned off for the rest of the run the first time a bulk copy fails, so an environment where it cannot work
    // degrades to the ordinary EF path instead of failing every batch.
    private bool _bulkInsertEnabled;

    // Every sub-projection here writes only the rows of the entity its event belongs to, so no two events of one
    // change compete for the same row and the order they arrive in does not matter. That buys the guarantee this
    // driver rests on: a batch is exactly the page the daemon delivered, nothing above it, so the position below is
    // "everything at or under this sequence has been applied" without qualification - and a page re-delivered after
    // the SQL Server commit landed but the Marten commit did not is skipped whole.
    protected override bool RequiresEmissionOrder => false;

    protected DbContextBackedRoadNetworkChangesProjection(
        IDbContextFactory<TDbContext> dbContextFactory,
        IReadOnlyCollection<IRoadNetworkChangesProjection<TDbContext>> projections,
        ILoggerFactory loggerFactory,
        int batchSize = DefaultBatchSize,
        ProjectionCatchUpOptions? catchUpOptions = null)
        : base(loggerFactory, batchSize)
    {
        _dbContextFactory = dbContextFactory;
        _projections = projections;
        _options = catchUpOptions ?? ProjectionCatchUpOptions.Default;
        _bulkInsertEnabled = _options.BulkInsertThreshold > 0;
        _metrics = new ProjectionThroughputMetrics(Logger, ProjectionName, _options.MetricsLogIntervalInBatches, _options.MetricsLogIntervalInMinutes);
    }

    protected override async Task DispatchAsync(IDocumentOperations operations, IReadOnlyList<CorrelationWorkItem> correlationWork, long pageMaxSequence, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        // The connection is configured to retry, which means this delegate can run more than once. Clearing the change
        // tracker at the top of each attempt is what makes a retry start from the same place the first attempt did.
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            context.ChangeTracker.Clear();
            await ApplyBatchAsync(context, correlationWork, pageMaxSequence, cancellationToken).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private async Task ApplyBatchAsync(TDbContext context, IReadOnlyList<CorrelationWorkItem> correlationWork, long pageMaxSequence, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var projectionState = await context.ProjectionStates.FindAsync([ProjectionName], cancellationToken);
        if (projectionState is null)
        {
            projectionState = new ProjectionStateItem { Name = ProjectionName };
            await context.ProjectionStates.AddAsync(projectionState, cancellationToken);

            // No projection state means the projection starts from nothing: a new read model, or one a rebuild emptied
            // (the rebuild removes the state row with the tables). The initialization commits with this first batch,
            // so a batch that fails initializes again when it is retried.
            foreach (var projection in _projections.OfType<IInitializableRoadNetworkChangesProjection<TDbContext>>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await projection.InitializeAsync(context, cancellationToken).ConfigureAwait(false);
            }
        }

        var position = projectionState.Position;
        var eventsProcessed = 0;

        var handlerDuration = Stopwatch.StartNew();

        // Offer the batch to the sub-projections that can read ahead on it, before any handler runs. Only the events
        // that will actually be applied: the ones at or below the position are skipped below, and reading rows for
        // those would be work for nothing. The time this takes is charged to the handlers, because that is where it
        // used to be spent - it is the same reads, done once instead of once per event.
        var eventsToProcess = _options.PrefetchBatches
            ? correlationWork
                .SelectMany(work => work.ToProcess)
                .Where(evt => evt.Sequence > position)
                .ToList()
            : [];
        if (eventsToProcess.Count > 0)
        {
            foreach (var projection in _projections.OfType<IBatchPrefetchingRoadNetworkChangesProjection<TDbContext>>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await projection.PrefetchBatchAsync(context, eventsToProcess, cancellationToken).ConfigureAwait(false);
            }
        }

        foreach (var work in correlationWork)
        {
            foreach (var evt in work.ToProcess)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Skip events already applied (and committed to SQL Server) before a re-delivery. Sound because the
                // position never runs ahead of the page the daemon delivered, and the batch never reaches past it.
                if (evt.Sequence <= position)
                {
                    Logger.LogInformation(
                        "Skipping event at sequence {Sequence} for {Projection} because the projection state position is already at {Position}.",
                        evt.Sequence, ProjectionName, position);
                    continue;
                }

                using var eventScope = Logger.BeginScope(new Dictionary<string, object> { ["EventTypeName"] = evt.EventTypeName, ["EventSequence"] = evt.Sequence });

                foreach (var projection in _projections)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    projection.IsCatchingUp = IsCatchingUp;
                    projection.Logger ??= Logger;

                    using var childProjectionScope = Logger.BeginScope(new Dictionary<string, object> { ["ChildProjectionName"] = projection.GetType().Name });
                    await projection.Project(context, [evt], cancellationToken).ConfigureAwait(false);
                }

                eventsProcessed++;
            }
        }
        handlerDuration.Stop();

        // The position is "everything at or below this sequence has been applied". With RequiresEmissionOrder off the
        // batch is exactly the page, so every event it applied sits at or below pageMaxSequence and that sentence is
        // simply true - which is what makes the guard above complete rather than partial.
        //
        // It moves forward even when the page held nothing this projection handles: those events are accounted for by
        // being irrelevant, and leaving the position behind them would only make the guard skip less than it could.
        var newPosition = Math.Max(position, pageMaxSequence);

        // Nothing applied and no ground gained; nothing to save.
        if (eventsProcessed == 0 && newPosition == position)
        {
            return;
        }

        // Advance the position right before saving, so it commits atomically with the read-model writes (even when the
        // batch produced no handler changes and only the position moved forward).
        cancellationToken.ThrowIfCancellationRequested();
        projectionState.Position = newPosition;
        context.ChangeTracker.DetectChanges();

        // Bulk copy is a SQL Server path: on a provider without relational metadata Collect itself would fail, so keep
        // the whole thing out of the way there.
        IReadOnlyList<SqlServerBulkInserter.BulkInsertBatch> bulkInsertBatches =
            _bulkInsertEnabled && context.Database.IsRelational()
                ? SqlServerBulkInserter.Collect(context, _options.BulkInsertThreshold)
                : [];

        // The explicit transaction is only here to make the bulk copy commit together with SaveChangesAsync. A batch
        // with nothing to bulk copy is a plain SaveChangesAsync, which is atomic on its own, so there is nothing to
        // span - and opening a transaction the batch does not need makes this projection unusable on a provider that
        // has none: the in-memory provider throws on BeginTransactionAsync, which pauses the shard.
        await using var transaction = bulkInsertBatches.Count > 0
            ? await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)
            : null;

        var bulkCopyDuration = new Stopwatch();
        var rowsBulkInserted = 0;
        if (bulkInsertBatches.Count > 0)
        {
            bulkCopyDuration.Start();
            try
            {
                await SqlServerBulkInserter.WriteAsync(context, bulkInsertBatches, cancellationToken).ConfigureAwait(false);
                rowsBulkInserted = SqlServerBulkInserter.RowCount(bulkInsertBatches);
                // Only once the rows are actually on the server, so a failure leaves them tracked for the EF path.
                SqlServerBulkInserter.Detach(context, bulkInsertBatches);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // The transaction cannot be trusted after a failed copy, so let the batch fail and be replayed -
                // with bulk insert switched off, the replay goes through EF and makes progress.
                _bulkInsertEnabled = false;
                Logger.LogWarning(ex,
                    "Bulk insert failed for {Projection}; falling back to the Entity Framework insert path for the remainder of this run. The batch will be replayed.",
                    ProjectionName);
                throw;
            }
            finally
            {
                bulkCopyDuration.Stop();
            }
        }

        var rowsViaEntityFramework = context.ChangeTracker.Entries().Count(entry => entry.State == EntityState.Added);

        var saveChangesDuration = Stopwatch.StartNew();
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        saveChangesDuration.Stop();

        _metrics.RecordBatch(
            eventsProcessed,
            rowsBulkInserted,
            rowsViaEntityFramework,
            handlerDuration.Elapsed,
            bulkCopyDuration.Elapsed,
            saveChangesDuration.Elapsed,
            IsCatchingUp);
    }

    protected override async Task OnCatchUpStartedAsync(long estimatedBacklog, CancellationToken cancellationToken)
    {
        await ForEachCatchUpAwareProjection(
            projection => projection.OnCatchUpStartedAsync(cancellationToken),
            cancellationToken).ConfigureAwait(false);

        // Say what state the read model's indexes are already in, before this run decides what to do with them. A
        // disabled index is indistinguishable from a missing one in a query plan, and it outlives the run that
        // disabled it: a host killed mid-catch-up leaves them off, and the next run - including one configured to
        // leave them alone - then spends its life scanning with nothing in the log to say why.
        await ReportIndexStateAsync(cancellationToken).ConfigureAwait(false);

        // Switched off, so this run not only leaves the indexes alone but puts back any an earlier run turned off.
        // Without that, the option cannot be used to recover: the indexes are disabled by one run and nothing in the
        // next one re-enables them, since the rebuild below only happens once a catch-up finishes - which, with every
        // lookup reduced to a scan, is exactly what will not happen within any useful time. Turning the option off
        // has to be enough on its own, because the operator who needs it may have no way to run DDL by hand.
        if (!_options.DisableIndexesWhileCatchingUp)
        {
            Logger.LogInformation(
                "{Projection} is catching up on about {Backlog} event(s) with index disabling switched off.",
                ProjectionName, estimatedBacklog);

            await RebuildDisabledIndexesAsync("because index disabling is switched off for this projection", cancellationToken).ConfigureAwait(false);
            return;
        }

        // Putting the indexes back afterwards is heavy work in its own right, so a short replay - a restart, a deploy -
        // must not pay for it. Only a backlog on the scale of a real rebuild earns it.
        if (estimatedBacklog < _options.MinimumBacklogForIndexDisabling)
        {
            Logger.LogInformation(
                "{Projection} is catching up on about {Backlog} event(s), below the {Minimum} needed to make disabling the indexes worthwhile; leaving them in place.",
                ProjectionName, estimatedBacklog, _options.MinimumBacklogForIndexDisabling);
            return;
        }

        // Index maintenance is an optimisation, not a correctness requirement - a database user without ALTER rights
        // must leave the projection running (slower), not pause the shard.
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            await SqlServerIndexMaintenance.DisableAsync(context, Logger, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogWarning(ex, "Could not disable the indexes for {Projection}; catching up with them left in place.", ProjectionName);
        }
    }

    protected override async Task OnCatchUpFinishedAsync(CancellationToken cancellationToken)
    {
        await ForEachCatchUpAwareProjection(
            projection => projection.OnCatchUpFinishedAsync(cancellationToken),
            cancellationToken).ConfigureAwait(false);

        // Unconditional on the option: a host that ran with it enabled, was killed, and came back with it disabled
        // must still put the indexes back. Costs one catalog query when there is nothing disabled.
        await RebuildDisabledIndexesAsync("now that the projection is caught up", cancellationToken).ConfigureAwait(false);
    }

    private async Task RebuildDisabledIndexesAsync(string reason, CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            await SqlServerIndexMaintenance.RebuildDisabledAsync(context, Logger, reason, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Worth shouting about: unlike the disable side, leaving indexes disabled degrades the served read model
            // until someone rebuilds them.
            Logger.LogError(ex, "Could not rebuild the disabled indexes for {Projection}. They must be rebuilt before the read model is served from.", ProjectionName);
        }
    }

    // Reporting only - it never changes anything, and a failure to read the catalog must not keep a projection from
    // starting, so it is logged and shrugged off like the maintenance below it.
    private async Task ReportIndexStateAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            if (!context.Database.IsRelational())
            {
                return;
            }

            var disabled = await SqlServerIndexMaintenance.FindDisabledAsync(context, cancellationToken).ConfigureAwait(false);
            if (disabled.Count == 0)
            {
                Logger.LogInformation("{Projection} starts with every non-clustered index on its read model in place.", ProjectionName);
                return;
            }

            Logger.LogWarning(
                "{Projection} starts with {Count} non-clustered index(es) on its read model disabled, so every lookup that needs one is a scan: {Indexes}",
                ProjectionName, disabled.Count, string.Join(", ", disabled));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogWarning(ex, "Could not read the index state for {Projection}.", ProjectionName);
        }
    }

    private async Task ForEachCatchUpAwareProjection(Func<IProjectionCatchUpAware, Task> action, CancellationToken cancellationToken)
    {
        foreach (var projection in _projections.OfType<IProjectionCatchUpAware>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(projection).ConfigureAwait(false);
        }
    }
}
