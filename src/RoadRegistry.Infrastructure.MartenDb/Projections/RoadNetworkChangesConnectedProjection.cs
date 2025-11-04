namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using JasperFx.Events;
using Marten;

public abstract class RoadNetworkChangesConnectedProjection : ConnectedProjection<IDocumentSession>, IRoadNetworkChangesProjection
{
    private readonly Lazy<ConnectedProjectionHandlerResolver<IDocumentSession>> _resolver;

    protected RoadNetworkChangesConnectedProjection()
    {
        _resolver = new Lazy<ConnectedProjectionHandlerResolver<IDocumentSession>>(() => Resolve.WhenAssignableToHandlerMessageType(Handlers));
    }

    public async Task Project(IReadOnlyList<IEvent> events, IDocumentSession session, CancellationToken cancellationToken)
    {
        foreach (var evt in events)
        foreach (var handler in _resolver.Value(evt))
        {
            await handler
                .Handler(session, evt, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
