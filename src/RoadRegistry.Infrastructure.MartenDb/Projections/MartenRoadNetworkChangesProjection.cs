namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using JasperFx.Events;
using Marten;
using Microsoft.Extensions.Logging;

public abstract class MartenRoadNetworkChangesProjection : ConnectedProjection<IDocumentOperations>, IRoadNetworkChangesProjection<IDocumentOperations>
{
    private readonly Lazy<ConnectedProjectionHandlerResolver<IDocumentOperations>> _resolver;

    public bool IsCatchingUp { get; set; }
    public ILogger? Logger { get; set; }

    protected MartenRoadNetworkChangesProjection()
    {
        _resolver = new Lazy<ConnectedProjectionHandlerResolver<IDocumentOperations>>(() => Resolve.WhenAssignableToHandlerMessageType(Handlers));
    }

    public async Task Project(IDocumentOperations session, IReadOnlyList<IEvent> events, CancellationToken cancellationToken)
    {
        foreach (var evt in events)
        {
            var handled = false;

            foreach (var handler in _resolver.Value(evt))
            {
                handled = true;
                await handler
                    .Handler(session, evt, cancellationToken)
                    .ConfigureAwait(false);
            }

            if (!handled)
            {
                // Not necessarily wrong - a sub-projection only handles the events it cares about - but it is the one
                // outcome that looks exactly like success while changing nothing, so it must leave a trace.
                Logger?.LogDebug("{Projection} has no handler for {EventType} at sequence {Sequence}",
                    GetType().Name, evt.EventTypeName, evt.Sequence);
            }
        }
    }
}
