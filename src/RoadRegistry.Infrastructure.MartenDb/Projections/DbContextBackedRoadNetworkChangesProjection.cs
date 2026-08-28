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
// write diverged; it replaces the per-sub-projection positions that used to exist.
//
// Two things speed up the write side, both under ProjectionCatchUpOptions, neither changing what is written:
//   - a batch's high-volume inserts are streamed with SqlBulkCopy instead of EF's per-row INSERT batching. This is
//     driven by how many rows of one type a batch holds, not by catching up, so a large live batch benefits too;
//   - the read model's non-clustered indexes are disabled while catching up on a backlog big enough to be worth it,
//     and rebuilt once the tail is reached.
// The commit stays atomic: the bulk copy runs on the context's own connection inside the same transaction as
// SaveChangesAsync, so the rows and the position still land together or not at all.
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
        _metrics = new ProjectionThroughputMetrics(Logger, ProjectionName, _options.MetricsLogIntervalInBatches);
    }

    protected override async Task DispatchAsync(IDocumentOperations operations, IReadOnlyList<CorrelationWorkItem> correlationWork, CancellationToken cancellationToken)
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
            await ApplyBatchAsync(context, correlationWork, cancellationToken).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private async Task ApplyBatchAsync(TDbContext context, IReadOnlyList<CorrelationWorkItem> correlationWork, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var projectionState = await context.ProjectionStates.FindAsync([ProjectionName], cancellationToken);
        if (projectionState is null)
        {
            projectionState = new ProjectionStateItem { Name = ProjectionName };
            await context.ProjectionStates.AddAsync(projectionState, cancellationToken);
        }

        var position = projectionState.Position;
        var newPosition = position;
        var eventsProcessed = 0;

        var handlerDuration = Stopwatch.StartNew();
        foreach (var work in correlationWork)
        {
            foreach (var evt in work.ToProcess)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Skip events already applied (and committed to SQL Server) before a re-delivery.
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
                newPosition = Math.Max(newPosition, evt.Sequence);
            }
        }
        handlerDuration.Stop();

        // Everything in this batch was already applied; nothing to advance or save.
        if (newPosition == position)
        {
            return;
        }

        // Advance the position right before saving, so it commits atomically with the read-model writes (even when the
        // batch produced no handler changes and only the position moved forward).
        cancellationToken.ThrowIfCancellationRequested();
        projectionState.Position = newPosition;
        context.ChangeTracker.DetectChanges();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var bulkCopyDuration = new Stopwatch();
        var rowsBulkInserted = 0;
        if (_bulkInsertEnabled)
        {
            var bulkInsertBatches = SqlServerBulkInserter.Collect(context, _options.BulkInsertThreshold);
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
        }

        var rowsViaEntityFramework = context.ChangeTracker.Entries().Count(entry => entry.State == EntityState.Added);

        var saveChangesDuration = Stopwatch.StartNew();
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
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

        if (!_options.DisableIndexesWhileCatchingUp)
        {
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
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            await SqlServerIndexMaintenance.RebuildDisabledAsync(context, Logger, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Worth shouting about: unlike the disable side, leaving indexes disabled degrades the served read model
            // until someone rebuilds them.
            Logger.LogError(ex, "Could not rebuild the disabled indexes for {Projection}. They must be rebuilt before the read model is served from.", ProjectionName);
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
