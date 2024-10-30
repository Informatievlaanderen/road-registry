namespace RoadRegistry.SyncHost.Tests;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;

public class ConnectedProjectionFixture<TProjection, TConnection>
    where TProjection : ConnectedProjection<TConnection>, new()
{
    public TConnection Connection { get; }
    private readonly ConnectedProjector<TConnection> _projector;

    public ConnectedProjectionFixture(TConnection connection, ConnectedProjectionHandlerResolver<TConnection> resolver)
    {
        Connection = connection;
        _projector = new ConnectedProjector<TConnection>(resolver);
    }

    public Task ProjectEnvelopeAsync<TMessage>(TMessage message)
        where TMessage : IMessage
    {
        return _projector.ProjectAsync(Connection, new Envelope<TMessage>(new Envelope(message, new Dictionary<string, object>())));
    }

    public Task ProjectAsync(object message)
    {
        return _projector.ProjectAsync(Connection, message, CancellationToken.None);
    }

    public Task ProjectAsync(object message, CancellationToken cancellationToken)
    {
        return _projector.ProjectAsync(Connection, message, cancellationToken);
    }

    public Task ProjectAsync(IEnumerable<object> messages)
    {
        return _projector.ProjectAsync(Connection, messages);
    }

    public Task ProjectAsync(IEnumerable<object> messages, CancellationToken cancellationToken)
    {
        return _projector.ProjectAsync(Connection, messages, cancellationToken);
    }
}
