namespace RoadRegistry.Framework
{
    using System;
    using System.Linq;

    public static class Resolve
    {
        public static CommandHandlerResolver WhenEqualToMessage(CommandHandlerModule[] modules) =>
            WhenEqualToMessage(modules.SelectMany(module => module.Handlers).ToArray());
        public static CommandHandlerResolver WhenEqualToMessage(CommandHandlerModule module) =>
            WhenEqualToMessage(module.Handlers);

        public static CommandHandlerResolver WhenEqualToMessage(CommandHandler[] handlers)
        {
            if (handlers == null)
            {
                throw new ArgumentNullException(nameof(handlers));
            }

            var cache = handlers.ToDictionary(handler => handler.Command);

            return message => {
                if(message == null)
                    throw new ArgumentNullException(nameof(message));
                
                if(!cache.TryGetValue(message.Body.GetType(), out CommandHandler handler))
                {
                    throw new InvalidOperationException($"The command handler for {message.Body.GetType()} could not be found.");
                }
                return handler;
            };
        }
    }
}