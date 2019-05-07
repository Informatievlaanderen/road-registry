namespace RoadRegistry.BackOffice.Framework
{
    using System;

    public static class Dispatch
    {
        public static CommandHandlerDispatcher Using(CommandHandlerResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            return (message, cancellationToken) => resolver(message).Handler(message, cancellationToken);
        }
    }
}