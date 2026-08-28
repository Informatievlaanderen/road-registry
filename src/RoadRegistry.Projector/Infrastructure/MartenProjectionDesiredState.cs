namespace RoadRegistry.Projector.Infrastructure;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using RoadRegistry.BackOffice;

// What an operator wants a Marten projection to be doing, as opposed to what its shard happens to be doing right now.
//
// The stream-store projections have carried this since the beginning, on their ProjectionStateItem: the status page
// reports the *desired* state, which is why its play icon means "should be running" rather than "is running". The Marten
// projections had no equivalent, so their state was read straight off the daemon - which cannot express intent, and
// which conflates a projection somebody stopped on purpose with one that fell over.
//
// Persisting it separately is what lets the supervisor leave a deliberately stopped projection alone, and what lets the
// status page tell "stopped, as intended" apart from "stopped, and nobody meant it to be".
public sealed class MartenProjectionDesiredState
{
    // The shard name, e.g. "RoadNetworkChangesReadProjection:All".
    public required string Id { get; set; }
    public required string DesiredState { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
}

// The vocabulary the projections status page already speaks for the stream-store projections; the Marten ones now use
// the same words so the page needs no special case.
public static class ProjectionDesiredStates
{
    public const string Subscribed = "subscribed";
    public const string Stopped = "stopped";
}

public sealed class MartenProjectionDesiredStateStore
{
    private readonly IDocumentStore _documentStore;

    public MartenProjectionDesiredStateStore(IDocumentStore documentStore)
    {
        _documentStore = documentStore;
    }

    public async Task<Dictionary<string, string>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var session = _documentStore.QuerySession();
        var states = await session.Query<MartenProjectionDesiredState>().ToListAsync(cancellationToken);
        return states.ToDictionary(x => x.Id, x => x.DesiredState);
    }

    // Absent means nobody has expressed an intent yet, which is the same as "leave it running" - the fallback the
    // projection was registered with.
    public async Task<string?> GetAsync(string shardName, CancellationToken cancellationToken)
    {
        await using var session = _documentStore.QuerySession();
        var state = await session.LoadAsync<MartenProjectionDesiredState>(shardName, cancellationToken);
        return state?.DesiredState;
    }

    public async Task SetAsync(string shardName, string desiredState, CancellationToken cancellationToken)
    {
        await using var session = _documentStore.LightweightSession();
        session.Store(new MartenProjectionDesiredState
        {
            Id = shardName,
            DesiredState = desiredState,
            ChangedAt = DateTimeOffset.UtcNow
        });
        await session.SaveChangesAsync(cancellationToken);
    }
}

public static class MartenProjectionDesiredStateExtensions
{
    public static StoreOptions ConfigureMartenProjectionDesiredState(this StoreOptions options)
    {
        options.Schema.For<MartenProjectionDesiredState>()
            .DatabaseSchemaName(WellKnownSchemas.MartenEventStore)
            .DocumentAlias("martenprojection_desiredstate")
            .Identity(x => x.Id);

        return options;
    }
}
