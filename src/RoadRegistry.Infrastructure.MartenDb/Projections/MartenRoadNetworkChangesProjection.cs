namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using JasperFx.Events;
using Marten;

public abstract class MartenRoadNetworkChangesProjection : ConnectedProjection<IDocumentOperations>, IRoadNetworkChangesProjection<IDocumentOperations>
{
    private readonly Lazy<ConnectedProjectionHandlerResolver<IDocumentOperations>> _resolver;

    public bool IsCatchingUp { get; set; }

    protected MartenRoadNetworkChangesProjection()
    {
        _resolver = new Lazy<ConnectedProjectionHandlerResolver<IDocumentOperations>>(() => Resolve.WhenAssignableToHandlerMessageType(Handlers));
    }

    public async Task Project(IDocumentOperations session, IReadOnlyList<IEvent> events, CancellationToken cancellationToken)
    {
        foreach (var evt in events)
        {
            foreach (var handler in _resolver.Value(evt))
            {
                await handler
                    .Handler(session, evt, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
