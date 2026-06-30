namespace RoadRegistry.Infrastructure.MartenDb;

using System.Data;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Store;

public static class DocumentStoreExtensions
{
    public static Task IdempotentSession(this IDocumentStore store, string sessionIdentifier, Action<IDocumentSession> action, CancellationToken cancellationToken, ILogger? logger = null)
    {
        return store.IdempotentSession(
            sessionIdentifier,
            session =>
            {
                action(session);
                return Task.CompletedTask;
            },
            cancellationToken,
            logger);
    }

    public static async Task IdempotentSession(this IDocumentStore store, string sessionIdentifier, Func<IDocumentSession, Task> action, CancellationToken cancellationToken, ILogger? logger = null)
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

        session.Insert(new IdempotentSession(sessionIdentifier));

        await session.SaveChangesAsync(cancellationToken);
    }
}
