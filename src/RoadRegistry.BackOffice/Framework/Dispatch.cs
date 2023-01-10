namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Linq;
using System.Threading.Tasks;

public static class Dispatch
{
    public static CommandHandlerDispatcher Using(CommandHandlerResolver resolver, ApplicationMetadata applicationMetadata = null)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        return (message, cancellationToken) => resolver(message)(message, applicationMetadata, cancellationToken);
    }

    public static EventHandlerDispatcher Using(EventHandlerResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        return (message, cancellationToken) =>
        {
            var handlers = resolver(message);
            return Task.WhenAll(handlers.Select(handler => handler(message, cancellationToken)));
        };
    }
}
