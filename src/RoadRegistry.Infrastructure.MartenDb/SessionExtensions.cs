namespace RoadRegistry.Infrastructure.MartenDb;

using BackOffice;
using Marten;
using GradeSeparatedJunction;
using JasperFx.Events;
using Marten.Events;
using Microsoft.Extensions.Logging;
using Polly;
using RoadNode;
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

    public static async Task<RoadNode?> LoadAsync(this IDocumentSession session, RoadNodeId id)
    {
        var result = await session.LoadManyAsync([id]);
        return result.SingleOrDefault();
    }

    public static async Task<RoadSegment?> LoadAsync(this IDocumentSession session, RoadSegmentId id)
    {
        var result = await session.LoadManyAsync([id]);
        return result.SingleOrDefault();
    }

    public static async Task<GradeSeparatedJunction?> LoadAsync(this IDocumentSession session, GradeSeparatedJunctionId id)
    {
        var result = await session.LoadManyAsync([id]);
        return result.SingleOrDefault();
    }

    public static async Task<IReadOnlyList<RoadSegment>> LoadManyAsync(this IDocumentSession session, IEnumerable<RoadSegmentId> ids)
    {
        return await session.LoadManyEntitiesAsync<RoadSegment, RoadSegmentId>(ids);
    }

    public static async Task<IReadOnlyList<RoadNode>> LoadManyAsync(this IDocumentSession session, IEnumerable<RoadNodeId> ids)
    {
        return await session.LoadManyEntitiesAsync<RoadNode, RoadNodeId>(ids);
    }

    public static async Task<IReadOnlyList<GradeSeparatedJunction>> LoadManyAsync(this IDocumentSession session, IEnumerable<GradeSeparatedJunctionId> ids)
    {
        return await session.LoadManyEntitiesAsync<GradeSeparatedJunction, GradeSeparatedJunctionId>(ids);
    }

    private static async Task<IReadOnlyList<TEntity>> LoadManyEntitiesAsync<TEntity, TIdentifier>(this IDocumentSession session, IEnumerable<TIdentifier> ids)
        where TEntity : MartenAggregateRootEntity<TIdentifier>
    {
        var streamKeys = ids.Select(x => StreamKeyFactory.Create(typeof(TEntity), x)).ToList();
        if (!streamKeys.Any())
        {
            return [];
        }

        var aggregates = (await session.LoadManyAsync<TEntity>(streamKeys)).ToList();

        foreach (var streamKey in streamKeys.Where(x => aggregates.All(snapshot => snapshot.Id != x)))
        {
            var aggregate = await session.Events.AggregateStreamAsync<TEntity>(streamKey);
            if (aggregate is not null)
            {
                aggregate.RequestToSaveSnapshot();

                aggregates.Add(aggregate);
            }
        }

        return aggregates.AsReadOnly();
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
