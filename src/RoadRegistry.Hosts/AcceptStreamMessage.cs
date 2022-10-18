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
    protected AcceptStreamMessage()
    {
    }

    public static AcceptStreamMessageFilter WhenEqualToMessageType(ConnectedProjection<TDbContext>[] projections, EventMapping mapping)
    {
        var acceptableEventNames = new HashSet<string>(
            projections
                .SelectMany(module => module.Handlers)
                .Select(handler => handler.Message)
                .Distinct()
                .Where(envelope => envelope.IsGenericType && envelope.GetGenericTypeDefinition() == typeof(Envelope<>))
                .Select(envelope => envelope.GenericTypeArguments[0])
                .Distinct()
                .Where(mapping.HasEventName)
                .Select(mapping.GetEventName)
        );
        return message => mapping.HasEventType(message.Type) && acceptableEventNames.Contains(message.Type);
    }
}