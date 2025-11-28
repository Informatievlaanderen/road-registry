namespace RoadRegistry.Wms.Projections.MartenDb;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Infrastructure.MartenDb.Projections;
using JasperFx.Events;
using Marten;
using Microsoft.EntityFrameworkCore;

public abstract class DbContextRoadNetworkChangesProjection<TDbContext> : IRoadNetworkChangesProjection
    where TDbContext : RunnerDbContext<TDbContext>
{
    private readonly string _projectionStateName;
    private readonly ConnectedProjectionHandlerResolver<TDbContext> _resolver;
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;

    protected DbContextRoadNetworkChangesProjection(
        string projectionStateName,
        ConnectedProjectionHandlerResolver<TDbContext> resolver,
        IDbContextFactory<TDbContext> dbContextFactory)
    {
        _projectionStateName = projectionStateName;
        _resolver = resolver;
        _dbContextFactory = dbContextFactory;
    }

    public async Task Project(IReadOnlyList<IEvent> events, IDocumentSession session, CancellationToken cancellationToken)
    {
        var eventsWithHandlers = events
            .Select(x => (Event: x, Handlers: _resolver(x)))
            .Where(x => x.Handlers.Any())
            .ToList();
        if (!eventsWithHandlers.Any())
        {
            return;
        }

        var newPosition = events.Last().Version;

        await using var processContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        processContext.ChangeTracker.AutoDetectChangesEnabled = false;

        var projectionState = await EntityFrameworkQueryableExtensions.SingleOrDefaultAsync(processContext.ProjectionStates, x => x.Name == _projectionStateName, cancellationToken)
            .ConfigureAwait(false);
        if (projectionState is not null && projectionState.Position >= newPosition)
        {
            return;
        }

        foreach (var eventWithHandlers in eventsWithHandlers)
        {
            foreach (var handler in eventWithHandlers.Handlers)
            {
                await handler
                    .Handler(processContext, eventWithHandlers.Event, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        await processContext.UpdateProjectionState(
            _projectionStateName,
            newPosition,
            cancellationToken).ConfigureAwait(false);

        processContext.ChangeTracker.DetectChanges();
        await processContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
