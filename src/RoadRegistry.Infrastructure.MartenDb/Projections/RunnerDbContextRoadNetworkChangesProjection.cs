namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
using JasperFx.Events;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public abstract class RunnerDbContextRoadNetworkChangesProjection<TDbContext> : ConnectedProjection<TDbContext>, IRoadNetworkChangesProjection
    where TDbContext : RunnerDbContext<TDbContext>
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly ILogger _logger;
    private readonly Lazy<ConnectedProjectionHandlerResolver<TDbContext>> _resolver;

    public bool IsCatchingUp { get; set; }

    protected RunnerDbContextRoadNetworkChangesProjection(
        IDbContextFactory<TDbContext> dbContextFactory,
        ILoggerFactory? loggerFactory = null)
    {
        _dbContextFactory = dbContextFactory;
        _logger = loggerFactory?.CreateLogger(GetType()) ?? NullLogger.Instance;
        _resolver = new Lazy<ConnectedProjectionHandlerResolver<TDbContext>>(() => Resolve.WhenAssignableToHandlerMessageType(Handlers));
    }

    protected RunnerDbContextRoadNetworkChangesProjection(
        ConnectedProjectionHandlerResolver<TDbContext> resolver,
        IDbContextFactory<TDbContext> dbContextFactory,
        ILoggerFactory? loggerFactory = null)
    {
        _dbContextFactory = dbContextFactory;
        _logger = loggerFactory?.CreateLogger(GetType()) ?? NullLogger.Instance;
        _resolver = new Lazy<ConnectedProjectionHandlerResolver<TDbContext>>(() => resolver);
    }

    private string ProjectionStateName => GetType().Name;

    public async Task Project(IDocumentOperations operations, IReadOnlyList<IEvent> events, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        var projectionState = await context.ProjectionStates.FindAsync([ProjectionStateName], cancellationToken);
        if (projectionState is null)
        {
            projectionState = new ProjectionStateItem { Name = ProjectionStateName };
            await context.ProjectionStates.AddAsync(projectionState, cancellationToken);
        }

        var position = projectionState.Position;
        var newPosition = position;

        foreach (var @event in events)
        {
            // Skip events already applied (and committed to SQL Server) before a re-delivery.
            if (@event.Sequence <= position)
            {
                _logger.LogInformation(
                    "Skipping event at sequence {Sequence} for {Projection} because the projection state position is already at {Position}.",
                    @event.Sequence, ProjectionStateName, position);
                continue;
            }

            foreach (var handler in _resolver.Value(@event))
            {
                await handler.Handler(context, @event, cancellationToken).ConfigureAwait(false);
            }

            newPosition = Math.Max(newPosition, @event.Sequence);
        }

        // Everything in this batch was already applied; nothing to advance or save.
        if (newPosition == position)
        {
            return;
        }

        // Advance the position right before saving, so it commits atomically with the read-model writes (even when the
        // batch produced no handler changes and only the position moved forward).
        projectionState.Position = newPosition;
        context.ChangeTracker.DetectChanges();
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
