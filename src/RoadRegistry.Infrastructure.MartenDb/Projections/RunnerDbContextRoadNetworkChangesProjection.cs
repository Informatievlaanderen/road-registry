namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using JasperFx.Events;

public abstract class RunnerDbContextRoadNetworkChangesProjection<TDbContext> : ConnectedProjection<TDbContext>, IRoadNetworkChangesProjection<TDbContext>
    where TDbContext : RunnerDbContext<TDbContext>
{
    private readonly Lazy<ConnectedProjectionHandlerResolver<TDbContext>> _resolver;

    public bool IsCatchingUp { get; set; }

    protected RunnerDbContextRoadNetworkChangesProjection()
    {
        _resolver = new Lazy<ConnectedProjectionHandlerResolver<TDbContext>>(() => Resolve.WhenAssignableToHandlerMessageType(Handlers));
    }

    protected RunnerDbContextRoadNetworkChangesProjection(ConnectedProjectionHandlerResolver<TDbContext> resolver)
    {
        _resolver = new Lazy<ConnectedProjectionHandlerResolver<TDbContext>>(() => resolver);
    }

    public async Task Project(TDbContext session, IReadOnlyList<IEvent> events, CancellationToken cancellationToken)
    {
        foreach (var @event in events)
        {
            foreach (var handler in _resolver.Value(@event))
            {
                await handler.Handler(session, @event, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
