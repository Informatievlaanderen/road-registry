namespace RoadRegistry.Projections.Tests.Projections.WmsWfsV2;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.WmsWfsV2.Projections;
using RoadRegistry.WmsWfsV2.Schema;

/// <summary>
/// Drives one or more PBS projections against an in-memory <see cref="WmsWfsV2Context"/>, mirroring how the Marten daemon
/// runs <c>RoadNetworkChangesWmsWfsV2Projection</c> over each event batch: every projection gets a fresh context from the
/// factory, applies its handlers and saves once. No SQL Server / Marten infrastructure is required.
/// The <c>IDocumentOperations</c> argument is unused by the PBS handlers (they write exclusively to the WmsWfsV2Context),
/// so <c>null</c> is passed for it.
/// </summary>
public sealed class WmsWfsV2ProjectionScenario
{
    private readonly TestDbContextFactory _dbContextFactory = new();
    private readonly IReadOnlyList<IRoadNetworkChangesProjection> _projections;
    private long _position;
    private IReadOnlyList<IEvent> _lastBatch = [];

    public WmsWfsV2ProjectionScenario(Func<IDbContextFactory<WmsWfsV2Context>, IEnumerable<IRoadNetworkChangesProjection>> projections)
    {
        _projections = projections(_dbContextFactory).ToList();
    }

    /// <summary>
    /// Projects the given messages (one batch) through every projection in order.
    /// Call multiple times to simulate separate daemon batches.
    /// </summary>
    public async Task GivenAsync(params object[] messages)
    {
        var events = messages.Select(BuildEvent).ToList();
        _lastBatch = events;

        foreach (var projection in _projections)
        {
            await projection.Project(null!, events, CancellationToken.None);
        }
    }

    /// <summary>
    /// Re-projects the previous <see cref="GivenAsync"/> batch with the SAME event sequences, mimicking the Marten
    /// daemon re-delivering events after a partial failure (the SQL Server write committed but the Marten progression
    /// commit did not). The projection-state idempotency guard should skip these already-applied events.
    /// </summary>
    public async Task RedeliverLastBatchAsync()
    {
        foreach (var projection in _projections)
        {
            await projection.Project(null!, _lastBatch, CancellationToken.None);
        }
    }

    /// <summary>
    /// Directly mutate the read model, for state a projection expects to already exist (e.g. the road segments a
    /// junction reads to compute its intersection point). Runs against its own context and saves.
    /// </summary>
    public async Task SeedAsync(Func<WmsWfsV2Context, Task> seed)
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
        // Sequence drives the PBS projection-state idempotency guard, so it must be a 1-based monotonic value (the
        // projection state starts at position 0). It keeps increasing across GivenAsync calls to mimic daemon batches.
        _position++;
        evt.Version = _position;
        evt.Sequence = _position;
        return evt;
    }

    // Each scenario gets its own isolated in-memory database (a private root + unique name). Every context created from
    // this factory shares that database, so writes from one projection's context are visible to the next, exactly like
    // the shared SQL Server read model in production.
    private sealed class TestDbContextFactory : IDbContextFactory<WmsWfsV2Context>
    {
        private readonly InMemoryDatabaseRoot _root = new();
        private readonly string _databaseName = Guid.NewGuid().ToString("N");

        public WmsWfsV2Context CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<WmsWfsV2Context>()
                .UseInMemoryDatabase(_databaseName, _root)
                .EnableSensitiveDataLogging()
                // Each scenario builds its own options, so across a test run EF creates more than its 20 internal
                // service providers; that is expected here and not a real leak, so silence the warning.
                .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))
                .Options;
            return new WmsWfsV2Context(options);
        }
    }
}
