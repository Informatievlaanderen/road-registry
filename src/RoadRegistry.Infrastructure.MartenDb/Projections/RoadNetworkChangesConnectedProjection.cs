namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using JasperFx.Events;
using Marten;

public abstract class RoadNetworkChangesConnectedProjection : ConnectedProjection<IDocumentOperations>, IRoadNetworkChangesProjection
{
    private readonly Lazy<ConnectedProjectionHandlerResolver<IDocumentOperations>> _resolver;

    protected RoadNetworkChangesConnectedProjection()
    {
        _resolver = new Lazy<ConnectedProjectionHandlerResolver<IDocumentOperations>>(() => Resolve.WhenAssignableToHandlerMessageType(Handlers));
    }

    public async Task Project(IReadOnlyList<IEvent> events, IDocumentOperations operations, CancellationToken cancellationToken)
    {
        foreach (var evt in events)
        {
            foreach (var handler in _resolver.Value(evt))
            {
                await handler
                    .Handler(operations, evt, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
