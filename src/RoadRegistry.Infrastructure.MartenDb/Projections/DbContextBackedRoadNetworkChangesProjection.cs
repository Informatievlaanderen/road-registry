namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
using JasperFx.Events;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// Drives a set of RunnerDbContext-backed sub-projections against a single SQL Server read model. For each daemon batch
// it creates one TDbContext, applies every sub-projection's handlers for every event, and commits once - advancing a
// single projection-state row (keyed by this projection's name) atomically with the read-model writes. That single
// position is the SQL-side idempotency guard for events re-delivered after the Marten progression and the SQL Server
// write diverged; it replaces the per-sub-projection positions that used to exist.
public abstract class DbContextBackedRoadNetworkChangesProjection<TDbContext> : RoadNetworkChangesProjection
    where TDbContext : RunnerDbContext<TDbContext>
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly IReadOnlyCollection<IRoadNetworkChangesProjection<TDbContext>> _projections;

    protected DbContextBackedRoadNetworkChangesProjection(
        IDbContextFactory<TDbContext> dbContextFactory,
        IReadOnlyCollection<IRoadNetworkChangesProjection<TDbContext>> projections,
        ILoggerFactory loggerFactory,
        int batchSize = DefaultBatchSize)
        : base(loggerFactory, batchSize)
    {
        _dbContextFactory = dbContextFactory;
        _projections = projections;
    }

    protected override async Task DispatchAsync(IDocumentOperations operations, IReadOnlyList<CorrelationWorkItem> correlationWork, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        var projectionState = await context.ProjectionStates.FindAsync([ProjectionName], cancellationToken);
        if (projectionState is null)
        {
            projectionState = new ProjectionStateItem { Name = ProjectionName };
            await context.ProjectionStates.AddAsync(projectionState, cancellationToken);
        }

        var position = projectionState.Position;
        var newPosition = position;

        foreach (var work in correlationWork)
        {
            foreach (var evt in work.ToProcess)
            {
                // Skip events already applied (and committed to SQL Server) before a re-delivery.
                if (evt.Sequence <= position)
                {
                    Logger.LogInformation(
                        "Skipping event at sequence {Sequence} for {Projection} because the projection state position is already at {Position}.",
                        evt.Sequence, ProjectionName, position);
                    continue;
                }

                Logger.LogInformation("Processing event {Sequence}: {EventTypeName}", evt.Sequence, evt.EventTypeName);

                foreach (var projection in _projections)
                {
                    projection.IsCatchingUp = IsCatchingUp;

                    await projection.Project(context, [evt], cancellationToken).ConfigureAwait(false);
                }

                newPosition = Math.Max(newPosition, evt.Sequence);
            }
        }

        // Everything in this batch was already applied; nothing to advance or save.
        if (newPosition == position)
        {
            return;
        }

        // Advance the position right before saving, so it commits atomically with the read-model writes (even when the
        // batch produced no handler changes and only the position moved forward).
        projectionState.Position = newPosition;
        context.ChangeTracker.DetectChanges();
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
