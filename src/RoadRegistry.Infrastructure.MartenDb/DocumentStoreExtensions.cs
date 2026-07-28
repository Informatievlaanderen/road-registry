namespace RoadRegistry.Infrastructure.MartenDb;

using System.Data;
using JasperFx.Events;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Store;

public static class DocumentStoreExtensions
{
    public static Task IdempotentSession(this IDocumentStore store, string sessionIdentifier, Action<IDocumentSession> action, CancellationToken cancellationToken, ILogger? logger = null, long? changeOrdinal = null)
    {
        return store.IdempotentSession(
            sessionIdentifier,
            session =>
            {
                action(session);
                return Task.CompletedTask;
            },
            cancellationToken,
            logger,
            changeOrdinal);
    }

    // Convenience overloads that place the change ordinal right after the identifier, so migration change-handlers
    // read as IdempotentSession(id, changeIndex, session => ..., token).
    public static Task IdempotentSession(this IDocumentStore store, string sessionIdentifier, long changeOrdinal, Action<IDocumentSession> action, CancellationToken cancellationToken, ILogger? logger = null)
    {
        return store.IdempotentSession(sessionIdentifier, action, cancellationToken, logger, changeOrdinal);
    }

    public static Task IdempotentSession(this IDocumentStore store, string sessionIdentifier, long changeOrdinal, Func<IDocumentSession, Task> action, CancellationToken cancellationToken, ILogger? logger = null)
    {
        return store.IdempotentSession(sessionIdentifier, action, cancellationToken, logger, changeOrdinal);
    }

    public static async Task IdempotentSession(this IDocumentStore store, string sessionIdentifier, Func<IDocumentSession, Task> action, CancellationToken cancellationToken, ILogger? logger = null, long? changeOrdinal = null)
    {
        await using var session = store.LightweightSession(IsolationLevel.Snapshot);

        var idempotentSession = await session.LoadAsync<IdempotentSession>(sessionIdentifier, cancellationToken);
        if (idempotentSession is not null)
        {
            logger?.LogInformation("Session with identifier '{SessionIdentifier}' is already processed, skipping.", sessionIdentifier);
            return;
        }

        logger?.LogInformation("Processing session with identifier '{SessionIdentifier}'.", sessionIdentifier);
        await action(session);

        if (changeOrdinal is not null)
        {
            StampChangeOrdinal(session, changeOrdinal.Value);
        }

        session.Insert(new IdempotentSession(sessionIdentifier));

        await session.SaveChangesAsync(cancellationToken);
    }

    // Stamps every event appended in this session with the emission ordinal (EventOrdinal header), so the read
    // projection replays them in true order. Used by the Marten migration, where each change is its own session
    // and its ordinal is the change's index within the RoadNetworkChangesAccepted batch.
    private static void StampChangeOrdinal(IDocumentSession session, long changeOrdinal)
    {
        foreach (var stream in session.PendingChanges.Streams())
        {
            foreach (var @event in stream.Events)
            {
                @event.SetHeader(EventOrdinal.HeaderKey, changeOrdinal);
            }
        }
    }
}
