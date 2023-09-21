namespace RoadRegistry.SyncHost.Tests.StreetName.Projections.Fixtures;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

public class ConnectedProjectionFixture<TProjection, TConnection>
    where TProjection : ConnectedProjection<TConnection>, new()
{
    private readonly TConnection _connection;
    private readonly ConnectedProjector<TConnection> _projector;

    public ConnectedProjectionFixture(TConnection connection, ConnectedProjectionHandlerResolver<TConnection> resolver)
    {
        _connection = connection;
        _projector = new ConnectedProjector<TConnection>(resolver);
    }

    public TProjection Projection { get; init; }

    public Task ProjectAsync(object message)
    {
        return _projector.ProjectAsync(_connection, message);
    }

    public Task ProjectAsync(object message, CancellationToken cancellationToken)
    {
        return _projector.ProjectAsync(_connection, message, cancellationToken);
    }

    public Task ProjectAsync(IEnumerable<object> messages)
    {
        return _projector.ProjectAsync(_connection, messages);
    }

    public Task ProjectAsync(IEnumerable<object> messages, CancellationToken cancellationToken)
    {
        return _projector.ProjectAsync(_connection, messages, cancellationToken);
    }
}