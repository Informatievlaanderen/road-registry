namespace RoadRegistry.Wms.Projections;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using JasperFx.Events;
using Marten;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.Infrastructure.MartenDb.Projections;

//TODO-pr dit testen in integration test 1-malig happy flow, of toch om te bekijken hoe het gebruikt gaat moeten worden qua registraties
public abstract class DbContextRoadNetworkChangesProjection<TDbContext> : IRoadNetworkChangesProjection
    where TDbContext : RunnerDbContext<TDbContext>
{
    private readonly ConnectedProjectionHandlerResolver<TDbContext> _resolver;
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;

    protected DbContextRoadNetworkChangesProjection(
        ConnectedProjectionHandlerResolver<TDbContext> resolver,
        IDbContextFactory<TDbContext> dbContextFactory)
    {
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

        await using var processContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        processContext.ChangeTracker.AutoDetectChangesEnabled = false;

        foreach (var eventWithHandlers in eventsWithHandlers)
        {
            foreach (var handler in eventWithHandlers.Handlers)
            {
                await handler
                    .Handler(processContext, eventWithHandlers.Event, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        processContext.ChangeTracker.DetectChanges();
        await processContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
