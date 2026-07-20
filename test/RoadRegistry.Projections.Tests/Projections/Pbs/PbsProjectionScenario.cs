namespace RoadRegistry.Projections.Tests.Projections.Pbs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
using JasperFx.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Pbs.Projections;
using RoadRegistry.Pbs.Schema;

/// <summary>
/// Drives one or more PBS sub-projections against an in-memory <see cref="PbsContext"/>, mirroring how
/// <see cref="RoadNetworkChangesPbsProjection"/> runs them: one shared context per batch, every projection applies its
/// handlers against that context, and a single projection-state row (keyed by the aggregate name) is advanced and saved
/// once. No SQL Server / Marten infrastructure is required.
/// </summary>
public sealed class PbsProjectionScenario
{
    // The single read-side projection-state key, matching RoadNetworkChangesPbsProjection in production.
    private const string ProjectionStateName = nameof(RoadNetworkChangesPbsProjection);

    private readonly TestDbContextFactory _dbContextFactory = new();
    private readonly IReadOnlyList<IRoadNetworkChangesProjection<PbsContext>> _projections;
    private long _position;
    private IReadOnlyList<IEvent> _lastBatch = [];

    public PbsProjectionScenario(params IRoadNetworkChangesProjection<PbsContext>[] projections)
    {
        _projections = projections;
    }

    /// <summary>
    /// Projects the given messages (one batch) through every projection in order.
    /// Call multiple times to simulate separate daemon batches.
    /// </summary>
    public Task GivenAsync(params object[] messages)
    {
        var events = messages.Select(BuildEvent).ToList();
        _lastBatch = events;
        return ApplyBatch(events);
    }

    /// <summary>
    /// Re-projects the previous <see cref="GivenAsync"/> batch with the SAME event sequences, mimicking the Marten
    /// daemon re-delivering events after a partial failure (the SQL Server write committed but the Marten progression
    /// commit did not). The projection-state idempotency guard should skip these already-applied events.
    /// </summary>
    public Task RedeliverLastBatchAsync() => ApplyBatch(_lastBatch);

    // Mirrors DbContextBackedRoadNetworkChangesProjection: one context, a single projection-state position guard shared
    // by all sub-projections, and a single save.
    private async Task ApplyBatch(IReadOnlyList<IEvent> events)
    {
        await using var context = _dbContextFactory.CreateDbContext();
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        var projectionState = await context.ProjectionStates.FindAsync(ProjectionStateName);
        if (projectionState is null)
        {
            projectionState = new ProjectionStateItem { Name = ProjectionStateName };
            await context.ProjectionStates.AddAsync(projectionState);
        }

        var position = projectionState.Position;
        var newPosition = position;

        foreach (var @event in events)
        {
            if (@event.Sequence <= position)
            {
                continue;
            }

            foreach (var projection in _projections)
            {
                await projection.Project(context, [@event], CancellationToken.None);
            }

            newPosition = Math.Max(newPosition, @event.Sequence);
        }

        if (newPosition == position)
        {
            return;
        }

        projectionState.Position = newPosition;
        context.ChangeTracker.DetectChanges();
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Directly mutate the read model, for state a projection expects to already exist (e.g. the road segments a
    /// junction reads to compute its intersection point). Runs against its own context and saves.
    /// </summary>
    public async Task SeedAsync(Func<PbsContext, Task> seed)
    {
        await using var context = _dbContextFactory.CreateDbContext();
        await seed(context);
        await context.SaveChangesAsync();
    }

    /// <summary>Loads a single record back by primary key from a fresh context.</summary>
    public async Task<T?> Find<T>(params object[] keyValues) where T : class
    {
        await using var context = _dbContextFactory.CreateDbContext();
        return await context.Set<T>().FindAsync(keyValues);
    }

    /// <summary>Reads all records of a type (optionally filtered) back from a fresh context.</summary>
    public async Task<List<T>> Query<T>(Func<IQueryable<T>, IQueryable<T>>? filter = null) where T : class
    {
        await using var context = _dbContextFactory.CreateDbContext();
        IQueryable<T> query = context.Set<T>().AsNoTracking();
        if (filter is not null)
        {
            query = filter(query);
        }
        return await query.ToListAsync();
    }

    private IEvent BuildEvent(object message)
    {
        var eventType = typeof(Event<>).MakeGenericType(message.GetType());
        var evt = (IEvent)Activator.CreateInstance(eventType, message)!;
        // Sequence drives the projection-state idempotency guard, so it must be a 1-based monotonic value (the projection
        // state starts at position 0). It keeps increasing across GivenAsync calls to mimic daemon batches.
        _position++;
        evt.Version = _position;
        evt.Sequence = _position;
        return evt;
    }

    // Each scenario gets its own isolated in-memory database (a private root + unique name). Every context created from
    // this factory shares that database, so writes from one projection's context are visible to the next, exactly like
    // the shared SQL Server read model in production.
    private sealed class TestDbContextFactory : IDbContextFactory<PbsContext>
    {
        private readonly InMemoryDatabaseRoot _root = new();
        private readonly string _databaseName = Guid.NewGuid().ToString("N");

        public PbsContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<PbsContext>()
                .UseInMemoryDatabase(_databaseName, _root)
                .EnableSensitiveDataLogging()
                // Each scenario builds its own options, so across a test run EF creates more than its 20 internal
                // service providers; that is expected here and not a real leak, so silence the warning.
                .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))
                .Options;
            return new PbsContext(options);
        }
    }
}
