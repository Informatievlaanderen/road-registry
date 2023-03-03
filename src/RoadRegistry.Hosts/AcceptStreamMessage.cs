namespace RoadRegistry.Hosts;

using System.Collections.Generic;
using System.Linq;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;

public static class AcceptStreamMessage
{
    public static AcceptStreamMessageFilter WhenEqualToMessageType(EventHandlerModule[] modules, EventMapping mapping)
    {
        var acceptableEventNames = new HashSet<string>(
            modules
                .SelectMany(module => module.Handlers)
                .Select(handler => handler.Event)
                .Distinct()
                .Where(mapping.HasEventName)
                .Select(mapping.GetEventName)
        );
        return message => mapping.HasEventType(message.Type) && acceptableEventNames.Contains(message.Type);
    }
}

public class AcceptStreamMessage<TDbContext>
{
    private readonly ConnectedProjection<TDbContext>[] _projections;
    private readonly EventMapping _mapping;

    protected AcceptStreamMessage()
    {
    }

    public AcceptStreamMessage(ConnectedProjection<TDbContext>[] projections, EventMapping mapping)
    {
        _projections = projections;
        _mapping = mapping;
    }

    public AcceptStreamMessageFilter CreateFilter()
    {
        var acceptableEventNames = new HashSet<string>(
            _projections
                .SelectMany(module => module.Handlers)
                .Select(handler => handler.Message)
                .Distinct()
                .Where(envelope => envelope.IsGenericType && envelope.GetGenericTypeDefinition() == typeof(Envelope<>))
                .Select(envelope => envelope.GenericTypeArguments[0])
                .Distinct()
                .Where(_mapping.HasEventName)
                .Select(_mapping.GetEventName)
        );
        return message => _mapping.HasEventType(message.Type) && acceptableEventNames.Contains(message.Type);
    }

    public static AcceptStreamMessageFilter WhenEqualToMessageType(ConnectedProjection<TDbContext>[] projections, EventMapping mapping)
        => new AcceptStreamMessage<TDbContext>(projections, mapping).CreateFilter();
}
