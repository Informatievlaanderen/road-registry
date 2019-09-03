namespace RoadRegistry.BackOffice.Framework
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public static class Dispatch
    {
        public static CommandHandlerDispatcher Using(CommandHandlerResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            return (message, cancellationToken) => resolver(message)(message, cancellationToken);
        }

        public static EventHandlerDispatcher Using(EventHandlerResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            return (message, cancellationToken) =>
            {
                var handlers = resolver(message);
                return Task.WhenAll(handlers.Select(handler => handler(message, cancellationToken)));
            };
        }
    }
}
