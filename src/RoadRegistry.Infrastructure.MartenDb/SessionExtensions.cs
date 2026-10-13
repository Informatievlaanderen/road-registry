namespace RoadRegistry.Infrastructure.MartenDb;

using System.Linq.Expressions;
using BackOffice;
using Marten;
using GradeSeparatedJunction;
using GradeJunction;
using JasperFx.Events;
using Marten.Events;
using Microsoft.Extensions.Logging;
using Polly;
using RoadNode;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadSegment;

public static class SessionExtensions
{
    public static StreamAction AppendOrStartStream(this IEventStoreOperations operations, string streamKey, object @event)
    {
        if (@event is ICreatedEvent)
        {
            return operations.StartStream(streamKey, @event);
        }

        return operations.Append(streamKey, @event);
    }

    public static async Task<RoadNode?> LoadAsync(this IDocumentSession session, RoadNodeId id, CancellationToken cancellationToken = default)
    {
        var result = await session.LoadManyAsync([id], cancellationToken);
        return result.SingleOrDefault();
    }

    public static async Task<RoadSegment?> LoadAsync(this IDocumentSession session, RoadSegmentId id, CancellationToken cancellationToken = default)
    {
        var result = await session.LoadManyAsync([id], cancellationToken);
        return result.SingleOrDefault();
    }

    public static async Task<GradeSeparatedJunction?> LoadAsync(this IDocumentSession session, GradeSeparatedJunctionId id, CancellationToken cancellationToken = default)
    {
        var result = await session.LoadManyAsync([id], cancellationToken);
        return result.SingleOrDefault();
    }

    public static async Task<ScopedRoadNetwork?> LoadAsync(this IDocumentSession session, ScopedRoadNetworkId id, CancellationToken cancellationToken = default)
    {
        return await session.Events.AggregateStreamAsync<ScopedRoadNetwork>(StreamKeyFactory.Create(typeof(ScopedRoadNetwork), id), token: cancellationToken);
    }

    public static async Task<IReadOnlyList<RoadSegment>> LoadManyAsync(this IDocumentSession session, IEnumerable<RoadSegmentId> ids, CancellationToken cancellationToken = default)
    {
        return await session.LoadManyEntitiesAsync<RoadSegment, RoadSegmentId>(ids, cancellationToken);
    }

    public static async Task<IReadOnlyList<RoadNode>> LoadManyAsync(this IDocumentSession session, IEnumerable<RoadNodeId> ids, CancellationToken cancellationToken = default)
    {
        return await session.LoadManyEntitiesAsync<RoadNode, RoadNodeId>(ids, cancellationToken);
    }

    public static async Task<IReadOnlyList<GradeSeparatedJunction>> LoadManyAsync(this IDocumentSession session, IEnumerable<GradeSeparatedJunctionId> ids, CancellationToken cancellationToken = default)
    {
        return await session.LoadManyEntitiesAsync<GradeSeparatedJunction, GradeSeparatedJunctionId>(ids, cancellationToken);
    }

    public static async Task<IReadOnlyList<GradeJunction>> LoadManyAsync(this IDocumentSession session, IEnumerable<GradeJunctionId> ids, CancellationToken cancellationToken = default)
    {
        return await session.LoadManyEntitiesAsync<GradeJunction, GradeJunctionId>(ids, cancellationToken);
    }

    private static async Task<IReadOnlyList<TEntity>> LoadManyEntitiesAsync<TEntity, TIdentifier>(this IDocumentSession session, IEnumerable<TIdentifier> ids, CancellationToken cancellationToken = default)
        where TEntity : MartenAggregateRootEntity<TIdentifier>
    {
        var streamKeys = ids.Select(x => StreamKeyFactory.Create(typeof(TEntity), x)).ToArray();
        if (!streamKeys.Any())
        {
            return [];
        }

        var aggregates = (await session.LoadManyAsync<TEntity>(cancellationToken, streamKeys)).ToList();

        foreach (var streamKey in streamKeys.Where(x => aggregates.All(snapshot => snapshot.Id != x)))
        {
            var aggregate = await session.Events.AggregateStreamAsync<TEntity>(streamKey, token: cancellationToken);
            if (aggregate is not null)
            {
                aggregate.RequestToSaveSnapshot();

                aggregates.Add(aggregate);
            }
        }

        return aggregates.AsReadOnly();
    }

    // Runs a Marten Linq query and returns its results. Marten's own ToListAsync() casts the queryable to its
    // internal MartenLinqQueryable<T>, so a plain Linq-to-Objects queryable - what the in-memory document store the
    // unit tests run on hands out - cannot go through it. Routing read-model queries through here keeps the query
    // expressions themselves testable without a database: in memory they are evaluated by Linq to Objects, against
    // Postgres they are translated by Marten.
    public static Task<IReadOnlyList<T>> ToReadOnlyListAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken)
        where T : notnull
    {
        if (queryable.Provider is EnumerableQuery)
        {
            return Task.FromResult<IReadOnlyList<T>>(queryable.ToList());
        }

        return queryable.ToListAsync(cancellationToken);
    }

    // Runs a read-model query that has to be SQL rather than Linq - because Marten translates the Linq form onto
    // something Postgres cannot answer from the index (see ReadModelQueries) - and takes the equivalent Linq
    // predicate for the in-memory document store the unit tests run on, which cannot execute SQL. The two are the
    // same filter written twice; the Linq one is never used against Postgres, and the integration tests cover the
    // SQL one.
    public static Task<IReadOnlyList<T>> QueryAsync<T>(
        this IQuerySession session,
        string whereFragment,
        Expression<Func<T, bool>> inMemoryEquivalent,
        CancellationToken cancellationToken,
        params object[] parameters)
        where T : notnull
    {
        var queryable = session.Query<T>();

        return queryable.Provider is EnumerableQuery
            ? queryable.Where(inMemoryEquivalent).ToReadOnlyListAsync(cancellationToken)
            : session.QueryAsync<T>(whereFragment, cancellationToken, parameters);
    }

    public static async Task<long> GetHighWaterMark(this IDocumentOperations operations, CancellationToken cancellationToken)
    {
        return (await operations.AdvancedSql.QueryAsync<long>($"SELECT last_seq_id FROM {WellKnownSchemas.MartenEventStore}.mt_event_progression WHERE name = '{MartenConstants.HighWaterMarkName}'", cancellationToken)).SingleOrDefault();
    }

    public static Task<IReadOnlyList<EventProgression>> GetEventProgressions(this IDocumentOperations operations, CancellationToken cancellationToken)
    {
        return operations.AdvancedSql.QueryAsync<EventProgression>($"select json_build_object('{nameof(EventProgression.Name)}', name, '{nameof(EventProgression.LastSequenceId)}', last_seq_id) from {WellKnownSchemas.MartenEventStore}.mt_event_progression", cancellationToken);
    }

    public static async Task WaitForNonStaleProjection(this IDocumentStore store, string projectionName, ILogger logger, CancellationToken cancellationToken)
    {
        await using var session = store.LightweightSession();

        var highWaterMarkSequenceId = await session.GetHighWaterMark(cancellationToken);
        if (highWaterMarkSequenceId == 0)
        {
            throw new InvalidOperationException("No projection state found for HighWaterMark.");
        }

        await Policy
            .HandleResult<long>(projectionSequenceId => projectionSequenceId < highWaterMarkSequenceId)
            .WaitAndRetryForeverAsync(retryAttempt =>
            {
                if (retryAttempt == 1)
                {
                    logger.LogInformation("Projection '{projectionName}' is stale, waiting...", projectionName);
                }

                return TimeSpan.FromSeconds(1);
            })
            .ExecuteAsync(
                async token =>
                {
                    var projectionSequenceId = (await session.AdvancedSql.QueryAsync<long>($"SELECT last_seq_id FROM {WellKnownSchemas.MartenEventStore}.mt_event_progression WHERE name = ?", token, projectionName)).Single();
                    return projectionSequenceId;
                },
                cancellationToken
            );
    }
}

public sealed record EventProgression(string Name, long LastSequenceId);
