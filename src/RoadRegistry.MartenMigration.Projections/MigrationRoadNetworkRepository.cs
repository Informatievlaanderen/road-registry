namespace RoadRegistry.MartenMigration.Projections;

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.MartenDb.Store;
using Marten;
using Microsoft.Extensions.Logging;

public class MigrationRoadNetworkRepository : RoadNetworkRepository
{
    private readonly ILogger _logger;

    public MigrationRoadNetworkRepository(IDocumentStore store, ILoggerFactory loggerFactory)
        : base(store)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public Task InIdempotentSession(string eventIdentifier, Action<IDocumentSession> action, CancellationToken token)
    {
        return InIdempotentSession(
            eventIdentifier,
            session =>
            {
                action(session);
                return Task.CompletedTask;
            },
            token);
    }

    public async Task InIdempotentSession(string eventIdentifier, Func<IDocumentSession, Task> action, CancellationToken token)
    {
        await using var session = Store.LightweightSession(IsolationLevel.Snapshot);

        var eventProcessed = await session.LoadAsync<MigratedEvent>(eventIdentifier, token);
        if (eventProcessed is not null)
        {
            return;
        }

        _logger.LogInformation("Processing event with identifier {EventIdentifier}", eventIdentifier);
        await action(session);

        session.Insert(new MigratedEvent(eventIdentifier));

        await session.SaveChangesAsync(token);
    }
}

public sealed record MigratedEvent(string Id);
