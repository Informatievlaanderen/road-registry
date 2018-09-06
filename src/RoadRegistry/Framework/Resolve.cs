namespace RoadRegistry.Framework
{
    using System;
    using System.Linq;

    public static class Resolve<TContext>
    {
        public static CommandHandlerResolver<TContext> WhenEqualToMessage(CommandHandlerModule<TContext>[] modules) =>
            WhenEqualToMessage(modules.SelectMany(module => module.Handlers).ToArray());
        public static CommandHandlerResolver<TContext> WhenEqualToMessage(CommandHandlerModule<TContext> module) =>
            WhenEqualToMessage(module.Handlers);

        public static CommandHandlerResolver<TContext> WhenEqualToMessage(CommandHandler<TContext>[] handlers)
        {
            if (handlers == null)
            {
                throw new ArgumentNullException(nameof(handlers));
            }

            var cache = handlers.ToDictionary(handler => handler.Command);

            return message => {
                if(message == null)
                    throw new ArgumentNullException(nameof(message));
                
                if(!cache.TryGetValue(message.Body.GetType(), out CommandHandler<TContext> handler))
                {
                    throw new InvalidOperationException($"The command handler for {message.Body.GetType()} could not be found.");
                }
                return handler;
            };
        }
    }
}